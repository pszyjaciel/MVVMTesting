
namespace Console_MVVMTesting.Messages
{
    public class MyPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PersonsAge { get; set; }
        public bool Neutralise { get; set; }

        public MyPerson(string firstName, string lastName, int personsAge, bool zlikwidowac)
        {
            FirstName = firstName;
            LastName = lastName;
            PersonsAge = personsAge;
            Neutralise = zlikwidowac;
        }

        public MyPerson(string firstName, string lastName, int personsAge)
        {
            FirstName = firstName;
            LastName = lastName;
            PersonsAge = personsAge;
        }

        public MyPerson(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public MyPerson(string firstName)
        {
            FirstName = firstName;
        }

        public MyPerson()
        {
        }


    }
}
