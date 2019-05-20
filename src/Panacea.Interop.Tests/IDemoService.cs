using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Interop.Tests
{
    public interface IDemoService
    {
        int Add(int a, int b);
    }

    public interface IDemoClient
    {
        Task<int> Add(int a, int b);
    }

    public class DemoService : IDemoService
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
