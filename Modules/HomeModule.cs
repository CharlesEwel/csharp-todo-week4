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
        Task newTask = new Task(Request.Form["description"], Request.Form["date"], Request.Form["category-id"]);
        newTask.Save();
        Console.WriteLine(newTask.GetDescription());
        List<Category> categoryList = Category.GetAll();
        return View["index.cshtml", categoryList];
      };
      Get["/task/all"] = _ => {
        List<Task> allTasks = Task.GetAll();
        return View["task_list.cshtml", allTasks];
      };

      Get["/category/{id}"] = parameters => {
        Category category = Category.Find(parameters.id);
        List<Task> tasksInCategory = category.GetTasks();
        return View["task_list.cshtml", tasksInCategory];
      };
    }
  }

}
