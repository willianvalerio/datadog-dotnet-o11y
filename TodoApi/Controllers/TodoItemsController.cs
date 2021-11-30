using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using Serilog;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Datadog.Trace;
using Serilog.Context;
using Prometheus;

namespace TodoApi.Controllers
{
    #region TodoController
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        readonly ILogger<TodoItemsController> _logger;

        private static readonly Gauge TotalTodoItems = Metrics
            .CreateGauge("todoitems_total", "Total Items, labelled by status", 
            new GaugeConfiguration{
                LabelNames = new[] { "iscompleted" }
            });
        private static readonly Counter AddedTodoItems = Metrics
            .CreateCounter("todoitems_added", "Total Items Added");
        private static readonly Counter DeletedTodoItems = Metrics
            .CreateCounter("todoitems_deleted", "Total Items Deleted");
        
        private static readonly Counter UpdatedTodoItems = Metrics
            .CreateCounter("todoitems_updated", "Total Items Updated");

        public TodoItemsController(TodoContext context, ILogger<TodoItemsController> logger)
        {
             _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context;
        }
        #endregion

        // GET: api/TodoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            
            _logger.LogInformation("Get All Items");
            return await _context.TodoItems.ToListAsync();
        }

        #region snippet_GetByID
        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                _logger.LogError("Object not found");
                return NotFound();
            }

            return todoItem;
        }
        #endregion

        #region snippet_Update
        // PUT: api/TodoItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }
            //Probably this is not the best way to do this, but it works :D
            var OldStatus = _context.Entry(todoItem).GetDatabaseValues().
                                GetValue<Boolean>("IsComplete").ToString().ToLower();
            _context.Entry(todoItem).State = EntityState.Modified;
          
            try
            {    
                await _context.SaveChangesAsync();
                using (LogContext.PushProperty("TodoItemId", todoItem.Id))
                using (LogContext.PushProperty("TodoItemName", todoItem.Name))
                {
                    _logger.LogInformation("Item updated");
                }
                TotalTodoItems.WithLabels(OldStatus).Dec();
                TotalTodoItems.WithLabels(todoItem.IsComplete.ToString().ToLower()).Inc();
                UpdatedTodoItems.Inc();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        #endregion

        #region snippet_Create
        // POST: api/TodoItems
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {           
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            using (LogContext.PushProperty("TodoItemId", todoItem.Id))
            using (LogContext.PushProperty("TodoItemName", todoItem.Name)){
                _logger.LogInformation("Item created");
            }

            TotalTodoItems.WithLabels(todoItem.IsComplete.ToString().ToLower()).Inc();
            AddedTodoItems.Inc();
            
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }
        #endregion

        #region snippet_Delete
        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }
            
            //Access the active scope through
            var scope = Tracer.Instance.ActiveScope;

             // the global tracer (can return null)
            if (scope != null){
                scope.Span.SetTag("item.id", todoItem.Id.ToString());
                scope.Span.SetTag("item.name", todoItem.Name);
            }

            //simulando um request externo
            using (var parentScope = 
                    Tracer.Instance.StartActive("http.request",serviceName:"apiexterna.com")){
                Thread.Sleep(1000);
                parentScope.Span.SetTag("http.method","GET");
                parentScope.Span.SetTag("http.status_code","200");
                parentScope.Span.ResourceName = "GET apiexterna.com/exemplo";
                parentScope.Span.Type="web";
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            using (LogContext.PushProperty("TodoItemId", todoItem.Id))
            using (LogContext.PushProperty("TodoItemName", todoItem.Name)){
                _logger.LogInformation("Item deleted");
            }

            TotalTodoItems.WithLabels(todoItem.IsComplete.ToString().ToLower()).Dec();
            DeletedTodoItems.Inc();

            return todoItem;
        }
        #endregion

        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
