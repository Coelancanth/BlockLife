using System;

namespace BlockLife.Core.Tests.Utils
{
    /// <summary>
    /// Provides consistent test GUIDs for predictable test data.
    /// </summary>
    public static class TestGuids
    {
        public static readonly Guid BlockA = new("11111111-1111-1111-1111-111111111111");
        public static readonly Guid BlockB = new("22222222-2222-2222-2222-222222222222");
        public static readonly Guid BlockC = new("33333333-3333-3333-3333-333333333333");
        public static readonly Guid BlockD = new("44444444-4444-4444-4444-444444444444");
        public static readonly Guid BlockE = new("55555555-5555-5555-5555-555555555555");
    }
}
