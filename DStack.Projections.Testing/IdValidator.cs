using System;

namespace DStack.Projections.Testing;

public static class IdValidator
{
    public static void ValidateType(object id)
    {
        string text = id as string;
        if (text != null)
        {
            return;
        }

        if (id is int)
        {
            int num = (int)id;
            return;
        }

        if (id is long)
        {
            long num2 = (long)id;
            return;
        }

        if (id is Guid)
        {
            Guid guid = (Guid)id;
            return;
        }

        throw new ArgumentException("Unsopported Id type!");
    }
}
