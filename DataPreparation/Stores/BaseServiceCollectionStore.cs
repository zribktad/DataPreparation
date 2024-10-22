using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPreparation.Testing
{
    internal  static class BaseServiceCollectionStore
    {

        internal static  IServiceCollection Base{ get; set; } = new ServiceCollection();
    }
}
