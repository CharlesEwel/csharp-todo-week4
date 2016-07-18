using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace ToDoList
{
  public class Category
  {
    private int _id;
    private string _name;

    public Category(string Name, int Id = 0)
    {
      _id = Id;
      _name = Name;
    }

    public override bool Equals(System.Object otherCategory)
    {
        if (!(otherCategory is Category))
        {
          return false;
        }
        else
        {
          Category newCategory = (Category) otherCategory;
          bool idEquality = this.GetId() == newCategory.GetId();
          bool nameEquality = this.GetName() == newCategory.GetName();
          return (idEquality && nameEquality);
        }
    }

    public int GetId()
    {
      return _id;
    }
    public string GetName()
    {
      return _name;
    }
    public void SetName(string newName)
    {
      _name = newName;
    }
    public static List<Category> GetAll()
    {
      List<Category> allCategories = new List<Category>{};

      SqlConnection conn = DB.Connection();
      SqlDataReader rdr = null;
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM categories;", conn);
      rdr = cmd.ExecuteReader();

      while(rdr.Read())
      {
        int categoryId = rdr.GetInt32(0);
        string categoryName = rdr.GetString(1);
        Category newCategory = new Category(categoryName, categoryId);
        allCategories.Add(newCategory);
      }

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }

      return allCategories;
    }

    public void Save()
    {
      SqlConnection conn = DB.Connection();
      SqlDataReader rdr;
      conn.Open();

      SqlCommand cmd = new SqlCommand("INSERT INTO categories (name) OUTPUT INSERTED.id VALUES (@CategoryName);", conn);

      SqlParameter nameParameter = new SqlParameter();
      nameParameter.ParameterName = "@CategoryName";
      nameParameter.Value = this.GetName();
      cmd.Parameters.Add(nameParameter);
      rdr = cmd.ExecuteReader();

      while(rdr.Read())
      {
        this._id = rdr.GetInt32(0);
      }
      if (rdr != null)
      {
        rdr.Close();
      }
      if(conn != null)
      {
        conn.Close();
      }
    }

    public void Delete()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("DELETE FROM categories WHERE id = @CategoryId; DELETE FROM categories_tasks WHERE category_id = @CategoryId;", conn);
      SqlParameter categoryIdParameter = new SqlParameter();
      categoryIdParameter.ParameterName = "@CategoryId";
      categoryIdParameter.Value = this.GetId();

      cmd.Parameters.Add(categoryIdParameter);
      cmd.ExecuteNonQuery();

      if (conn != null)
      {
        conn.Close();
      }
    }

    public static void DeleteAll()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();
      SqlCommand cmd = new SqlCommand("DELETE FROM categories; DELETE FROM categories_tasks;", conn);
      cmd.ExecuteNonQuery();
    }

    public static Category Find(int id)
    {
      SqlConnection conn = DB.Connection();
      SqlDataReader rdr = null;
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM categories WHERE id = @CategoryId;", conn);
      SqlParameter categoryIdParameter = new SqlParameter();
      categoryIdParameter.ParameterName = "@CategoryId";
      categoryIdParameter.Value = id.ToString();
      cmd.Parameters.Add(categoryIdParameter);
      rdr = cmd.ExecuteReader();

      int foundCategoryId = 0;
      string foundCategoryDescription = null;

      while(rdr.Read())
      {
        foundCategoryId = rdr.GetInt32(0);
        foundCategoryDescription = rdr.GetString(1);
      }
      Category foundCategory = new Category(foundCategoryDescription, foundCategoryId);

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
      return foundCategory;
    }

    public void AddTask(Task newTask)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlDataReader rdrCheckForDuplicate = null;
      SqlCommand cmdCheckForDuplicate = new SqlCommand("SELECT * FROM categories_tasks WHERE category_id=@CategoryId AND task_id=@TaskId", conn);
      SqlParameter categoryIdParameterCheckForDuplicate = new SqlParameter();
      categoryIdParameterCheckForDuplicate.ParameterName = "@CategoryId";
      categoryIdParameterCheckForDuplicate.Value = this.GetId();
      cmdCheckForDuplicate.Parameters.Add(categoryIdParameterCheckForDuplicate);

      SqlParameter taskIdParameterCheckForDuplicate = new SqlParameter();
      taskIdParameterCheckForDuplicate.ParameterName = "@TaskId";
      taskIdParameterCheckForDuplicate.Value = newTask.GetId();
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
        SqlCommand cmd = new SqlCommand("INSERT INTO categories_tasks (category_id, task_id) VALUES (@CategoryId, @TaskId)", conn);

        SqlParameter categoryIdParameter = new SqlParameter();
        categoryIdParameter.ParameterName = "@CategoryId";
        categoryIdParameter.Value = this.GetId();
        cmd.Parameters.Add(categoryIdParameter);

        SqlParameter taskIdParameter = new SqlParameter();
        taskIdParameter.ParameterName = "@TaskId";
        taskIdParameter.Value = newTask.GetId();
        cmd.Parameters.Add(taskIdParameter);

        cmd.ExecuteNonQuery();
      }
      if (conn != null)
      {
        conn.Close();
      }

    }

    public List<Task> GetTasks()
    {
      SqlConnection conn = DB.Connection();
      SqlDataReader rdr = null;
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT task_id FROM categories_tasks WHERE category_id = @CategoryId;", conn);
      SqlParameter categoryIdParameter = new SqlParameter();
      categoryIdParameter.ParameterName = "@CategoryId";
      categoryIdParameter.Value = this.GetId();
      cmd.Parameters.Add(categoryIdParameter);

      rdr = cmd.ExecuteReader();

      List<int> taskIds = new List<int> {};
      while(rdr.Read())
      {
        int taskId = rdr.GetInt32(0);
        taskIds.Add(taskId);
      }
      if (rdr != null)
      {
        rdr.Close();
      }

      List<Task> tasks = new List<Task> {};
      foreach (int taskId in taskIds)
      {
        SqlDataReader queryReader = null;
        SqlCommand taskQuery = new SqlCommand("SELECT * FROM tasks WHERE id = @TaskId;", conn);

        SqlParameter taskIdParameter = new SqlParameter();
        taskIdParameter.ParameterName = "@TaskId";
        taskIdParameter.Value = taskId;
        taskQuery.Parameters.Add(taskIdParameter);

        queryReader = taskQuery.ExecuteReader();
        while(queryReader.Read())
        {
              int thisTaskId = queryReader.GetInt32(0);
              string taskDescription = queryReader.GetString(1);
              DateTime taskDateTime = queryReader.GetDateTime(2);
              bool taskIsDone = queryReader.GetBoolean(3);
              Task foundTask = new Task(taskDescription, taskDateTime, taskIsDone, thisTaskId);
              tasks.Add(foundTask);
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
      return tasks;
    }



  }
}
