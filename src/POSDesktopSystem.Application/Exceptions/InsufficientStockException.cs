using System;

namespace POSDesktopSystem.Application.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(string message) : base(message) { }
}
