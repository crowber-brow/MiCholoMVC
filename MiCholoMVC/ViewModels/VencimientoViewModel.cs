namespace MiCholoMVC.ViewModels;
public class VencimientoViewModel
{
    public int IdLote {get;set;}
    public string Producto {get;set;}="";
    public string NumeroLote {get;set;}="";
    public int Cantidad {get;set;}
    public decimal CostoUnitario {get;set;}
    public decimal ValorTotal=>Cantidad*CostoUnitario;
    public DateTime FechaVencimiento {get;set;}
    public int DiasRestantes=>(FechaVencimiento.Date-MiCholoMVC.Services.ReglasNegocio.Hoy).Days;
    public string Grupo=>DiasRestantes<0?"VENCIDO":DiasRestantes==0?"HOY":DiasRestantes<=7?"7":DiasRestantes<=15?"15":DiasRestantes<=30?"30":"VIGENTE";
    public string EstadoVencimiento=>Grupo switch {"VENCIDO"=>"VENCIDO","HOY"=>"VENCE HOY","7"=>"VENCE EN 7 DÍAS","15"=>"VENCE EN 15 DÍAS","30"=>"VENCE EN 30 DÍAS",_=>"VIGENTE"};
    public string Color=>Grupo switch {"VENCIDO"=>"danger","HOY" or "7"=>"warning","15" or "30"=>"info",_=>"success"};
}
