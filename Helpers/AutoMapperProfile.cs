using AutoMapper;
using core8_svelte_sybase.Entities;
using core8_svelte_sybase.Models.dto;

namespace core8_svelte_sybase.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<UserRegister, User>();
            CreateMap<UserLogin, User>();
            CreateMap<UserUpdate, User>();
            CreateMap<UserPasswordUpdate, User>();
            CreateMap<Product, ProductModel>();
             CreateMap<ProductModel, Product>();

        }
    }
    

}