// (c) Microsoft. All rights reserved
namespace HealthVault.Foundation
{
    public static class HResults
    {
        public const uint FACILITY_ITF = 4;
        public const uint SEVERITY_ERROR = 1;
        public const uint S_OK = 0;
        public const uint E_UNEXPECTED = 0x8000FFFF;
        public const uint E_FAIL = 0x80004005;

        public const uint ErrorBase = 0x80040000;
        public const uint ClientErrorBase = ErrorBase | 0x00001000;
        public const uint ServerErrorBase = ErrorBase | 0x00002000;

        public static int Make(uint code)
        {
            return (int) (((SEVERITY_ERROR) << 31) | ((FACILITY_ITF) << 16) | (code));
        }
    }
}