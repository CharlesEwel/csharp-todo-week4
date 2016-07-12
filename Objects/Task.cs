using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace ToDoList
{
  public class Task
  {
    private int _id;
    private int _categoryId;
    private string _description;
    private DateTime? _date;

    public Task(string Description, DateTime? date, int categoryId, int Id = 0)
    {
      _id = Id;
      _description = Description;
      _categoryId = categoryId;
      _date = date;
    }

    public int GetId()
    {
      return _id;
    }

    public int GetCategoryId()
    {
      return _categoryId;
    }

    public string GetDescription()
    {
      return _description;
    }

    public DateTime? GetDate()
    {
      return _date;
    }

    public void SetDescription(string newDescription)
    {
      _description = newDescription;
    }

    public void SetCategoryId(int categoryId)
    {
      _categoryId = categoryId;
    }

    public void SetDate(DateTime? newDate)
    {
      _date = newDate;
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
        int taskCategoryId = rdr.GetInt32(2);
        string taskDescription = rdr.GetString(1);
        DateTime? taskDate = rdr.GetDateTime(3);
        Task newTask = new Task(taskDescription, taskDate, taskCategoryId, taskId);
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
      SqlCommand cmd = new SqlCommand("DELETE FROM tasks;", conn);
      cmd.ExecuteNonQuery();
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
        bool categoryEquality = (this.GetCategoryId() == newTask.GetCategoryId());
        bool dateTimeEquality = (this.GetDate() == newTask.GetDate());
        return (idEquality && descriptionEquality && categoryEquality && dateTimeEquality);
      }
    }

    public void Save()
  {
    SqlConnection conn = DB.Connection();
    SqlDataReader rdr;
    conn.Open();

    SqlCommand cmd = new SqlCommand("INSERT INTO tasks (description, category_id, date_time) OUTPUT INSERTED.id VALUES (@TaskDescription, @TaskCategoryId, @TaskDateTime);", conn);

    SqlParameter descriptionParameter = new SqlParameter();
    descriptionParameter.ParameterName = "@TaskDescription";
    descriptionParameter.Value = this.GetDescription();

    SqlParameter categoryIdParameter = new SqlParameter();
    categoryIdParameter.ParameterName = "@TaskCategoryId";
    categoryIdParameter.Value = this.GetCategoryId();

    SqlParameter dateTimeParameter = new SqlParameter();
    dateTimeParameter.ParameterName = "@TaskDateTime";
    dateTimeParameter.Value = this.GetDate();



    cmd.Parameters.Add(descriptionParameter);
    cmd.Parameters.Add(categoryIdParameter);
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
    int foundTaskCategoryId = 0;
    DateTime? foundTaskDateTime = null;

    while(rdr.Read())
    {
      foundTaskId = rdr.GetInt32(0);
      foundTaskDescription = rdr.GetString(1);
      foundTaskCategoryId = rdr.GetInt32(2);
      foundTaskDateTime = rdr.GetDateTime(3);
    }
    Task foundTask = new Task(foundTaskDescription, foundTaskDateTime, foundTaskCategoryId, foundTaskId);

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
  }
}
