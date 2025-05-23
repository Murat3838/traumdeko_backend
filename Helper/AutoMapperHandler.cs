using AutoMapper;
using VeragWebApp.Modal;
using VeragWebApp.Repos.Models;

namespace VeragWebApp.Helper
{
    public class AutoMapperHandler:Profile
    {
        public AutoMapperHandler() {
    
            CreateMap<TblUser, UserModel>().ForMember(item => item.Statusname, opt => opt.MapFrom(
                item => (item.isGekuendigt != null && item.isGekuendigt.Value) ? "Active" : "In active")).ReverseMap();
        }
    }
}
