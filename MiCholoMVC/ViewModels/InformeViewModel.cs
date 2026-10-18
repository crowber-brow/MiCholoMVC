using Microsoft.AspNetCore.Mvc.Rendering;
namespace MiCholoMVC.ViewModels;
public class InformeViewModel
{
    public string Tipo {get;set;}="ventas";
    public string Titulo {get;set;}="";
    public DateTime? Desde {get;set;}
    public DateTime? Hasta {get;set;}
    public int? IdProducto {get;set;}
    public int? IdCategoria {get;set;}
    public int? IdCaja {get;set;}
    public string? Motivo {get;set;}
    public string? Grupo {get;set;}
    public bool IncluirAnuladas {get;set;}
    public List<SelectListItem> Productos {get;set;}=[];
    public List<SelectListItem> Categorias {get;set;}=[];
    public List<SelectListItem> Cajas {get;set;}=[];
    public List<string> Columnas {get;set;}=[];
    public List<string[]> Filas {get;set;}=[];
    public List<string> Totales {get;set;}=[];
    public string Periodo => $"Desde: {Desde?.ToString("dd/MM/yyyy") ?? "inicio"} | Hasta: {Hasta?.ToString("dd/MM/yyyy") ?? "actualidad"}";
}
