using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace TygrISFramework
{
    [OracleCustomTypeMapping("ST64113.OSETROVATEL")]
    internal class Osetrovatel : IOracleCustomType
    {
        [OracleObjectMapping("jmeno")]
        public string Jmeno { get; set; }


        [OracleObjectMapping("prijmeni")]
        public string Prijmeni { get; set; }

        [OracleObjectMapping("rc")]
        public int RC { get; set; }


        [OracleObjectMapping("telefon")]
        public int Telefon { get; set; }


        [OracleObjectMapping("mail")]
        public string Mail { get; set; }


        [OracleObjectMapping("hruba_mzda")]
        public int HrubaMzda { get; set; }

        public void FromCustomObject(OracleConnection con, object udt)
        {
            OracleUdt.SetValue(con, udt, "jmeno", this.Jmeno);
            OracleUdt.SetValue(con, udt, "prijmeni", this.Prijmeni);
            OracleUdt.SetValue(con, udt, "rc", this.RC);
            OracleUdt.SetValue(con, udt, "telefon", this.Telefon);
            OracleUdt.SetValue(con, udt, "mail", this.Mail);
            OracleUdt.SetValue(con, udt, "hruba_mzda", this.HrubaMzda);
        }

        public void ToCustomObject(OracleConnection con, object udt)
        {
            this.Jmeno = (string)OracleUdt.GetValue(con, udt, "jmeno");
            this.Prijmeni = (string)OracleUdt.GetValue(con, udt, "prijmeni");
            this.RC = (int)OracleUdt.GetValue(con, udt, "rc");
            this.Telefon = (int)OracleUdt.GetValue(con, udt, "telefon");
            this.Mail = (string)OracleUdt.GetValue(con, udt, "mail");
            this.HrubaMzda = (int)OracleUdt.GetValue(con, udt, "hruba_mzda");
        }
    }
}
