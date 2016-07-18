using Nancy;
using System.Collections.Generic;
using System;

namespace ToDoList
{
  public class HomeModule : NancyModule
  {
    public HomeModule()
    {
      Get["/"] = _ => {
        List<Category> categoryList = Category.GetAll();
        return View["index.cshtml", categoryList];
      };

      Delete["/"]=_=>{
        Category.DeleteAll();
        Task.DeleteAll();
        List<Category> categoryList = Category.GetAll();
        return View["index.cshtml", categoryList];
      };

      Get["/category/add"] = _ => View["add_category.cshtml"];

      Post["/category/add"] = _ => {
        Category newCategory = new Category(Request.Form["name"]);
        newCategory.Save();
        List<Category> categoryList = Category.GetAll();
        return View["index.cshtml", categoryList];
      };

      Get["/task/add"] = _ => {
        List<Category> categoryList = Category.GetAll();
        return View["add_task.cshtml", categoryList];
      };

      Post["/task/add"] = _ => {
        Task newTask = new Task(Request.Form["description"], Request.Form["date"]);
        newTask.Save();
        List<Category> categoryList = Category.GetAll();
        return View["index.cshtml", categoryList];
      };

      Get["/task/all"] = _ => {
        List<Task> allTasks = Task.GetAll();
        return View["task_list.cshtml", allTasks];
      };

      Get["/category/{id}"] = parameters => {
        Category currentCategory = Category.Find(parameters.id);
        Console.WriteLine(currentCategory.GetName());
        List<Task> tasksInCategory = currentCategory.GetTasks();
        Console.WriteLine(tasksInCategory.Count);
        return View["task_list.cshtml", tasksInCategory];
      };

      Get["task/{id}"] = parameters => {
        Dictionary<string, object> model = new Dictionary<string, object>();
        Task SelectedTask = Task.Find(parameters.id);
        List<Category> TaskCategories = SelectedTask.GetCategories();
        List<Category> AllCategories = Category.GetAll();
        model.Add("task", SelectedTask);
        model.Add("taskCategories", TaskCategories);
        model.Add("allCategories", AllCategories);
        return View["task.cshtml", model];
      };

      Get["category/{id}"] = parameters => {
        Dictionary<string, object> model = new Dictionary<string, object>();
        Category SelectedCategory = Category.Find(parameters.id);
        List<Task> CategoryTasks = SelectedCategory.GetTasks();
        List<Task> AllTasks = Task.GetAll();
        model.Add("category", SelectedCategory);
        model.Add("categoryTasks", CategoryTasks);
        model.Add("allTasks", AllTasks);
        return View["category.cshtml", model];
      };

      Post["task/add_category"] = _ => {
        Category category = Category.Find(Request.Form["category-id"]);
        Task task = Task.Find(Request.Form["task-id"]);
        task.AddCategory(category);
        Dictionary<string, object> model = new Dictionary<string, object>();
        model.Add("task", task);
        model.Add("taskCategories", task.GetCategories());
        model.Add("allCategories", Category.GetAll());
        return View["task.cshtml", model];
      };

      Post["category/add_task"] = _ => {
        Category category = Category.Find(Request.Form["category-id"]);
        Task task = Task.Find(Request.Form["task-id"]);
        category.AddTask(task);
        Dictionary<string, object> model = new Dictionary<string, object>();
        model.Add("category", category);
        model.Add("categoryTasks", category.GetTasks());
        model.Add("allTasks", Task.GetAll());
        return View["category.cshtml", model];
      };

      Delete["category/{id}/delete"] = parameters =>
      {
        Category category = Category.Find(parameters.id);
        category.Delete();
        List<Category> categoryList = Category.GetAll();
        return View["index.cshtml", categoryList];
      };

      Delete["task/{id}/delete"] = parameters =>
      {
        Task task = Task.Find(parameters.id);
        task.Delete();
        List<Category> categoryList = Category.GetAll();
        return View["index.cshtml", categoryList];
      };

      Patch["task/{id}/complete"] = parameters =>
      {
        Task task = Task.Find(parameters.id);
        task.Complete();
        Dictionary<string, object> model = new Dictionary<string, object>();
        List<Category> TaskCategories = task.GetCategories();
        List<Category> AllCategories = Category.GetAll();
        model.Add("task", task);
        model.Add("taskCategories", TaskCategories);
        model.Add("allCategories", AllCategories);
        return View["task.cshtml", model];
      };


    }
  }

}
