using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.CustomCode
{
    public partial interface IUwCatalogService
    {

        //UWProductCatalog GetProductCatalog(int categoryId);
        //Task<UWProductCatalog> GetProductCatalog(int categoryId);
        Task<UWProductCatalog> GetProductCatalog(int categoryId);
        //Task<string> GetProductCatalog(int categoryId);
    }
}
