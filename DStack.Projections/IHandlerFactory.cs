using System;

namespace DStack.Projections;

public interface IHandlerFactory
{
    IHandler Create(Type t);
}