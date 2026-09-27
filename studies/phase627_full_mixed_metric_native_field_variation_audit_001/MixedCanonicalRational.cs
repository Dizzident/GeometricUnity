#if A68_GUARDED_EXACT_ARITHMETIC
using System.Numerics;

// Prospective guarded wire parser. No Split, substring, BigInteger.Parse or
// formatting allocation occurs before admission. The input string already
// exists: its JSON decoding/storage must be reserved by the enclosing reader.
// ReadBytes counts its UTF16 payload cumulatively; syntax + digit scans have
// <=4*Length character visits (including short-wire boundary inspections).
// Primitive integer guards precede every digit
// operation. These logical bounds do NOT cover BigInteger/GCD internals or RSS.
internal static class MixedCanonicalRational
{
 public static Rational Parse(string? text,int characterLimit)
 {
  var admission=MixedProducerAdmission.Current;
  try
  {
   if(characterLimit is <=0 or >16384||text is null||text.Length==0||text.Length>characterLimit)
    admission.Reject("canonical rational character ceiling/domain");
   // Reject above before reading even the first character. Length is Int32;
   // the multiplication is promoted before computing the UTF16 byte count.
   admission.ReadBytes(2L*text!.Length);
   bool negative=text[0]=='-';int first=negative?1:0,slash=-1;
   for(int i=first;i<text.Length;i++)
   {
    char c=text[i];
    if(c=='/'&&slash<0){slash=i;continue;}
    if(c<'0'||c>'9')admission.Reject("canonical ASCII rational syntax");
   }
   int numeratorEnd=slash<0?text.Length:slash;
   if(first==numeratorEnd||(text[first]=='0'&&(negative||numeratorEnd-first!=1)))
    admission.Reject("canonical numerator syntax");
   if(slash>=0&&(slash==text.Length-1||text[slash+1]=='0'||text[first]=='0'||
      (text.Length-slash==2&&text[slash+1]=='1')))
    admission.Reject("canonical positive denominator syntax");
   var ten=admission.FromLong(10);
   BigInteger ReadDigits(int start,int end)
   {
    BigInteger value=BigInteger.Zero;
    for(int i=start;i<end;i++)
    {
     var digit=admission.FromLong(text[i]-'0');
     admission.Binary("multiply",value,ten);var shifted=value*ten;
     admission.Binary("add",shifted,digit);value=shifted+digit;
    }
    return value;
   }
   var numerator=ReadDigits(first,numeratorEnd);
   if(negative){admission.Unary(numerator);numerator=-numerator;}
   var denominator=slash<0?BigInteger.One:ReadDigits(slash+1,text.Length);
   var result=new Rational(numerator,denominator);
   // A canonical fraction is already reduced. Compare both complete integers,
   // avoiding a second formatted copy just to test wire canonicality.
   admission.Compare(numerator,result.Numerator);admission.Compare(denominator,result.Denominator);
   if(numerator!=result.Numerator||denominator!=result.Denominator)
    admission.Reject("nonreduced rational wire");
   return result;
  }
  catch{admission.Poison();throw;}
 }
}
#endif
