namespace ELearning.Core.Common.Constants;

public static class ValidationConstants
{
    // Password ít nhất 8 ký tự, có 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt
    public const string PasswordRegexPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
    public const double MinScore = 0.0;
    public const double MaxScore = 10.0;

    public const string PasswordErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên, bao gồm ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt.";
    public const string ScoreErrorMessage = "Điểm số phải nằm trong khoảng từ 0 đến 10.";
}
