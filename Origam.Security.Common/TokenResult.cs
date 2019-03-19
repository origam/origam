namespace Origam.Security.Common
{
    public class TokenResult
    {
        public string ErrorMessage{ get; set; }
        public string Token{ get; set; }
        public int TokenValidityHours{ get; set; }
        public string UserName { get; set; }
    }
}