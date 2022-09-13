using System;

namespace Taurus.Plugin.Auth
{
    internal class AuthUsers
    {
        private string _TableName = "";
        public string TableName { get { return _TableName; } set { _TableName = value; } }
        private string _UserName = "UserName";
        public string UserName { get { return _UserName; } set { _UserName = value; } }
        private string _FullName = "FullName";
        public string FullName { get { return _FullName; } set { _FullName = value; } }
        private string _Password = "Password";
        public string Password { get { return _Password; } set { _Password = value; } }
        private string _Status = "Status";
        public string Status { get { return _Status; } set { _Status = value; } }
        private string _PasswordExpireTime = "PasswordExpireTime";
        public string PasswordExpireTime { get { return _PasswordExpireTime; } set { _PasswordExpireTime = value; } }
        private string _Email = "Email";
        public string Email { get { return _Email; } set { _Email = value; } }
        private string _Mobile = "Mobile";
        public string Mobile { get { return _Mobile; } set { _Mobile = value; } }
        private string _RoleID = "RoleID";
        public string RoleID { get { return _RoleID; } set { _RoleID = value; } }
        private int _TokenExpireTime = 24 * 365;
        public int TokenExpireTime { get { return _TokenExpireTime; } set { _TokenExpireTime = value; } }
    }
}
