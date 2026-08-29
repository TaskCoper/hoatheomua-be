namespace HoaTheoMua.Constants;

public class RoleNames
{
    public const string User = nameof(User);
    public const string AuthenticatedOnly = nameof(AuthenticatedOnly);
    public const string Mentor = nameof(Mentor);
    public const string Lecturer = nameof(Lecturer);
    public const string Admin = nameof(Admin);
    public const string SystemAdmin = nameof(SystemAdmin);
    public const string MentorOrLecturer = nameof(MentorOrLecturer);
    public const string ChangePassword = nameof(ChangePassword);

    // Added for hoatheomua-be unification (it owns Staff-gated endpoints).
    public const string Staff = nameof(Staff);
    public const string StaffOrAdmin = nameof(StaffOrAdmin);

    // Issued by taskcoper-be at registration; only the storefront (this service) consumes it.
    public const string Customer = nameof(Customer);
}