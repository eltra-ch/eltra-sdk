namespace EltraXamCommon.Controls.Toast
{
    public interface IToastMessage
    {
        void LongAlert(string message);
        void ShortAlert(string message);
    }
}