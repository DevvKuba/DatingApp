namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        // this DateOnly => can be called off his type
        public static int CalculateAge(this DateOnly dob)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var age = today.Year - dob.Year;

            // checks if person has had their birthday this year
            if (dob > today.AddYears(-age))
            {
                age--;
            }

            return age;



        }
    }
}
