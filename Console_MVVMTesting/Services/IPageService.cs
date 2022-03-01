using System;

namespace Console_MVVMTesting.Services
{
    public interface IPageService
    {
        Type GetPageType(string key);
    }
}
