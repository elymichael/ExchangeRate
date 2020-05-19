namespace ExchangeRate
{    
    using System.Collections.Generic;
    
    public class ExchangeRate
    {
        public string Rate { get; set; }
        public string Buy { get; set; }
        public string Sell { get; set; }
    }

    public class Rates : List<ExchangeRate>
    {

    }

    public class BankRates
    {
        public string Name { get; set; }
        public Rates Rates { get; set; }

        public BankRates(string name)
        {
            Name = name;
            Rates = new Rates();
        }
    }
}
