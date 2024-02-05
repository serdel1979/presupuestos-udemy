using AutoMapper;
using WebApplication1.Models;

namespace WebApplication1.Servicios
{
    public class AutomaperProfile: Profile
    {
        public AutomaperProfile()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();
            CreateMap<TransaccionActualizacionViewModel, Transaccion>().ReverseMap();
        }
    }
}
