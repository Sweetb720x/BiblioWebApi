using BiblioWebApi.Application.Catalog;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Controllers;

[ApiController, Route("api/categories")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [AllowAnonymous, HttpGet]
    public Task<IReadOnlyCollection<Category>> Get(CancellationToken ct) => sender.Send(new ListCategoriesQuery(), ct);
    [Authorize(Roles = "Administrator,Librarian"), HttpPost]
    public async Task<ActionResult<Category>> Create(CreateCategoryRequest request, CancellationToken ct) { var category = await sender.Send(new CreateCategoryCommand(request), ct); return Created($"/api/categories/{category.Id}", category); }
}


