namespace ExaminationSystem.Abstractions.Consts;

public static class DefaultRoles
{
    public partial class Admin
    {
        public const string Name = nameof(Admin);
        public const string Id = "019ba93d-a01b-71bf-af81-5096da8f8db5";
        public const string ConcurrencyStamp = "019ba93d-a01b-71bf-af81-509734aecad5";
    }
    public partial class StudentRole
    {
        public const string Name = nameof(StudentRole);
        public const string Id = "019ba93d-a01b-71bf-af81-50984737d2c3";
        public const string ConcurrencyStamp = "019ba93d-a01b-71bf-af81-50991e76e435";
    }
    public partial class InstructorRole
    {
        public const string Name = nameof(InstructorRole);
        public const string Id = "019ba93d-a01b-71bf-af81-509ad8b55b0f";
        public const string ConcurrencyStamp = "019ba93d-a01b-71bf-af81-509bb656fa2e";
    }
}
