using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace ToDoList
{
  public class Task
  {
    private int _id;
    private string _description;
    private DateTime? _date;
    private bool _isDone;

    public Task(string Description, DateTime? date, int Id = 0)
    {
      _id = Id;
      _description = Description;
      _date = date;
      _isDone = false;
    }

    public Task(string Description, DateTime? date, bool isDone, int Id = 0)
    {
      _id = Id;
      _description = Description;
      _date = date;
      _isDone = isDone;
    }

    public int GetId()
    {
      return _id;
    }

    public string GetDescription()
    {
      return _description;
    }

    public DateTime? GetDate()
    {
      return _date;
    }
    public bool IsDone()
    {
      return _isDone;
    }

    public void SetDescription(string newDescription)
    {
      _description = newDescription;
    }

    public void SetDate(DateTime? newDate)
    {
      _date = newDate;
    }
    public void Complete()
    {
      _isDone = true;

      SqlConnection conn = DB.Connection();
      conn.Open();
      SqlCommand cmd = new SqlCommand("UPDATE tasks SET is_done = 1 WHERE id = @IdParameter;", conn);

      SqlParameter idParameter = new SqlParameter();
      idParameter.ParameterName = "@IdParameter";
      idParameter.Value = this.GetId();
      cmd.Parameters.Add(idParameter);

      cmd.ExecuteNonQuery();
    }

    public static List<Task> GetAll()
    {
      List<Task> allTasks = new List<Task>{};

      SqlConnection conn = DB.Connection();
      SqlDataReader rdr = null;
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM tasks ORDER BY date_time;", conn);
      rdr = cmd.ExecuteReader();

      while(rdr.Read())
      {
        int taskId = rdr.GetInt32(0);
        string taskDescription = rdr.GetString(1);
        DateTime? taskDate = rdr.GetDateTime(2);
        bool taskIsDone = rdr.GetBoolean(3);
        Task newTask = new Task(taskDescription, taskDate, taskIsDone, taskId);
        allTasks.Add(newTask);
      }

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }

      return allTasks;
    }

    public static void DeleteAll()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();
      SqlCommand cmd = new SqlCommand("DELETE FROM tasks; DELETE FROM categories_tasks;", conn);
      cmd.ExecuteNonQuery();
    }

    public void Delete()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("DELETE FROM tasks WHERE id = @TaskId; DELETE FROM categories_tasks WHERE task_id = @TaskId;", conn);
      SqlParameter taskIdParameter = new SqlParameter();
      taskIdParameter.ParameterName = "@TaskId";
      taskIdParameter.Value = this.GetId();

      cmd.Parameters.Add(taskIdParameter);
      cmd.ExecuteNonQuery();

      if (conn != null)
      {
        conn.Close();
      }
    }

    public override bool Equals(System.Object otherTask)
    {
      if (!(otherTask is Task))
      {
        return false;
      }
      else
      {
        Task newTask = (Task) otherTask;
        bool idEquality = (this.GetId() == newTask.GetId());
        bool descriptionEquality = (this.GetDescription() == newTask.GetDescription());
        bool dateTimeEquality = (this.GetDate() == newTask.GetDate());
        bool completionEquality = (this.IsDone() == newTask.IsDone());
        return (idEquality && descriptionEquality && dateTimeEquality && completionEquality );
      }
    }

    public void Save()
    {
      SqlConnection conn = DB.Connection();
      SqlDataReader rdr;
      conn.Open();

      SqlCommand cmd = new SqlCommand("INSERT INTO tasks (description, date_time, is_done) OUTPUT INSERTED.id VALUES (@TaskDescription, @TaskDateTime, @TaskIsDone);", conn);

      SqlParameter descriptionParameter = new SqlParameter();
      descriptionParameter.ParameterName = "@TaskDescription";
      descriptionParameter.Value = this.GetDescription();


      SqlParameter dateTimeParameter = new SqlParameter();
      dateTimeParameter.ParameterName = "@TaskDateTime";
      dateTimeParameter.Value = this.GetDate();

      SqlParameter isDoneParameter = new SqlParameter();
      isDoneParameter.ParameterName = "@TaskIsDone";
      isDoneParameter.Value = this.IsDone();


      cmd.Parameters.Add(descriptionParameter);
      cmd.Parameters.Add(isDoneParameter);
      cmd.Parameters.Add(dateTimeParameter);

      rdr = cmd.ExecuteReader();

      while(rdr.Read())
      {
        this._id = rdr.GetInt32(0);
      }
      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
    }

    public static Task Find(int id)
    {
      SqlConnection conn = DB.Connection();
      SqlDataReader rdr = null;
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM tasks WHERE id = @TaskId ORDER BY date_time;", conn);
      SqlParameter taskIdParameter = new SqlParameter();
      taskIdParameter.ParameterName = "@TaskId";
      taskIdParameter.Value = id.ToString();
      cmd.Parameters.Add(taskIdParameter);
      rdr = cmd.ExecuteReader();


      int foundTaskId = 0;
      string foundTaskDescription = null;
      DateTime? foundTaskDateTime = null;
      bool foundTaskIsDone = false;

      while(rdr.Read())
      {
        foundTaskId = rdr.GetInt32(0);
        foundTaskDescription = rdr.GetString(1);
        foundTaskDateTime = rdr.GetDateTime(2);
        foundTaskIsDone = rdr.GetBoolean(3);
      }
      Task foundTask = new Task(foundTaskDescription, foundTaskDateTime, foundTaskIsDone, foundTaskId);

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
      return foundTask;
    }

    public void AddCategory(Category newCategory)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlDataReader rdrCheckForDuplicate = null;
      SqlCommand cmdCheckForDuplicate = new SqlCommand("SELECT * FROM categories_tasks WHERE category_id=@CategoryId AND task_id=@TaskId", conn);
      SqlParameter categoryIdParameterCheckForDuplicate = new SqlParameter();
      categoryIdParameterCheckForDuplicate.ParameterName = "@CategoryId";
      categoryIdParameterCheckForDuplicate.Value = newCategory.GetId();
      cmdCheckForDuplicate.Parameters.Add(categoryIdParameterCheckForDuplicate);

      SqlParameter taskIdParameterCheckForDuplicate = new SqlParameter();
      taskIdParameterCheckForDuplicate.ParameterName = "@TaskId";
      taskIdParameterCheckForDuplicate.Value = this.GetId();
      cmdCheckForDuplicate.Parameters.Add(taskIdParameterCheckForDuplicate);

      List<int> matchedJoins = new List<int> {};
      rdrCheckForDuplicate = cmdCheckForDuplicate.ExecuteReader();

      while(rdrCheckForDuplicate.Read())
      {
        matchedJoins.Add(rdrCheckForDuplicate.GetInt32(0));
      }
      if (rdrCheckForDuplicate != null) rdrCheckForDuplicate.Close();

      if (matchedJoins.Count == 0)
      {
        SqlCommand cmd = new SqlCommand("INSERT INTO categories_tasks (category_id, task_id) VALUES (@CategoryId, @TaskId);", conn);

        SqlParameter categoryIdParameter = new SqlParameter();
        categoryIdParameter.ParameterName = "@CategoryId";
        categoryIdParameter.Value = newCategory.GetId();
        cmd.Parameters.Add(categoryIdParameter);

        SqlParameter taskIdParameter = new SqlParameter();
        taskIdParameter.ParameterName = "@TaskId";
        taskIdParameter.Value = this.GetId();
        cmd.Parameters.Add(taskIdParameter);

        cmd.ExecuteNonQuery();
      }

      if (conn != null)
      {
        conn.Close();
      }
    }

    public List<Category> GetCategories()
    {
      SqlConnection conn = DB.Connection();
      SqlDataReader rdr = null;
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT category_id FROM categories_tasks WHERE task_id = @TaskId;", conn);

      SqlParameter taskIdParameter = new SqlParameter();
      taskIdParameter.ParameterName = "@TaskId";
      taskIdParameter.Value = this.GetId();
      cmd.Parameters.Add(taskIdParameter);

      rdr = cmd.ExecuteReader();

      List<int> categoryIds = new List<int> {};

      while (rdr.Read())
      {
        int categoryId = rdr.GetInt32(0);
        categoryIds.Add(categoryId);
      }
      if (rdr != null)
      {
        rdr.Close();
      }

      List<Category> categories = new List<Category> {};

      foreach (int categoryId in categoryIds)
      {
        SqlDataReader queryReader = null;
        SqlCommand categoryQuery = new SqlCommand("SELECT * FROM categories WHERE id = @CategoryId;", conn);

        SqlParameter categoryIdParameter = new SqlParameter();
        categoryIdParameter.ParameterName = "@CategoryId";
        categoryIdParameter.Value = categoryId;
        categoryQuery.Parameters.Add(categoryIdParameter);

        queryReader = categoryQuery.ExecuteReader();
        while (queryReader.Read())
        {
          int thisCategoryId = queryReader.GetInt32(0);
          string categoryName = queryReader.GetString(1);
          Category foundCategory = new Category(categoryName, thisCategoryId);
          categories.Add(foundCategory);
        }
        if (queryReader != null)
        {
          queryReader.Close();
        }
      }
      if (conn != null)
      {
        conn.Close();
      }
      return categories;
    }

  }
}
