using System.Globalization;
using System.Numerics;

// Manufactured wire inputs only; no study assembly, source tensors or files.
static class Program
{
 static readonly MixedProducerAdmission.Limits Generous=new(4096,16384,1000000,100000000,1000000,1000000,1000000,1000000,1000000,128,1000000,new(1000000,1000000,1000000,1000000,1000000,1000000,10000000,1000000,10000000,1000000),new(64,100000,1000000,1000000,100000,1000000));
 static int count;
 static void Need(bool condition,string message){if(!condition)throw new Exception(message);}
 static void Test(string name,Action test){test();count++;Console.WriteLine("PASS "+name);}
 static void Refuses(Action action){try{action();}catch(MixedProducerAdmission.Refused){return;}throw new Exception("expected refusal");}
 static void Main()
 {
  Test("mandatory live scope even for zero",()=>Refuses(()=>_=MixedCanonicalRational.Parse("0",1)));
  Test("canonical values and complete negative fraction",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);
   foreach(string wire in new[]{"0","1","-1","17","-123456789012345678901234567890","2/3","-17/19","12345678901234567890123456789/100000000000000000000000000000"})
   {
    var actual=MixedCanonicalRational.Parse(wire,512);var parts=wire.Split('/');
    Need(actual.Numerator==BigInteger.Parse(parts[0],CultureInfo.InvariantCulture)&&actual.Denominator==(parts.Length==1?BigInteger.One:BigInteger.Parse(parts[1],CultureInfo.InvariantCulture)),"full exact integer values");
   }
   Need(s.Snapshot().FormatCharacters==0,"parser does not reformat canonical values");
  });
  Test("all small coprime rational wires",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);
   for(int n=-35;n<=35;n++)for(int d=1;d<=29;d++)
   {
    if(BigInteger.GreatestCommonDivisor(n,d)!=1)continue;
    string wire=d==1?n.ToString(CultureInfo.InvariantCulture):$"{n}/{d}";
    var result=MixedCanonicalRational.Parse(wire,64);Need(result.Numerator==n&&result.Denominator==d,"coprime exact identity");
   }
  });
  Test("bad syntax rejects before any integer construction",()=>
  {
   foreach(string wire in new[]{"","-","+1","01","-0","-01","1/","/2","1//2","1/0","1/01","1/1","0/2","1/-2","1/+2"," 1","1 ","1.0","1e2","１","١","1\n","1\0"})
   {
    using var s=MixedProducerAdmission.Enter(Generous);Refuses(()=>_=MixedCanonicalRational.Parse(wire,512));
    Need(s.Snapshot().IntegerOperations==0&&s.Snapshot().GcdCalls==0&&s.Snapshot().Poisoned,"syntax precedes integer work");
   }
  });
  Test("unreduced fractions poison without normalization acceptance",()=>
  {
   foreach(string wire in new[]{"2/4","-9/6","12/3"})
   {using var s=MixedProducerAdmission.Enter(Generous);Refuses(()=>_=MixedCanonicalRational.Parse(wire,512));Need(s.Snapshot().GcdCalls==1&&s.Snapshot().Poisoned,"guarded reduction detects noncanonical input");Refuses(()=>_=MixedCanonicalRational.Parse("1",1));}
  });
  Test("character ceiling and null reject before input scan",()=>
  {
   foreach((string? wire,int cap) in new (string?,int)[]{(null,1),("123",2),("1",0),("1",16385)})
   {using var s=MixedProducerAdmission.Enter(Generous);Refuses(()=>_=MixedCanonicalRational.Parse(wire,cap));Need(s.Snapshot().InputBytes==0&&s.Snapshot().IntegerOperations==0,"no input admitted");}
  });
  Test("UTF16 cumulative input admission precedes digit arithmetic",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous with{InputBytes=4});_=MixedCanonicalRational.Parse("12",2);
   var before=s.Snapshot();Refuses(()=>_=MixedCanonicalRational.Parse("3",1));var after=s.Snapshot();
   Need(before.InputBytes==4&&after.InputBytes==4&&after.IntegerOperations==before.IntegerOperations,"cumulative byte bound before next parse");
  });
  Test("integer object cap refuses before decimal construction",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous with{IntegerObjects=1});Refuses(()=>_=MixedCanonicalRational.Parse("123",3));
   Need(s.Snapshot().IntegerObjects==1&&s.Snapshot().RationalConstructions==0&&s.Snapshot().GcdCalls==0,"no rational or GCD before refused digit");
  });
  Test("intermediate bit cap refuses oversized decimal before GCD",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous with{IntegerBits=64,IntermediateBits=64});Refuses(()=>_=MixedCanonicalRational.Parse("999999999999999999999999999999999999",64));
   Need(s.Snapshot().RationalConstructions==0&&s.Snapshot().GcdCalls==0,"no oversized parse then check");
  });
  Test("ASCII canonical parser is independent of locale",()=>
  {
   var old=CultureInfo.CurrentCulture;
   try
   {var locale=(CultureInfo)CultureInfo.InvariantCulture.Clone();locale.NumberFormat.NegativeSign="MINUS";CultureInfo.CurrentCulture=locale;using var s=MixedProducerAdmission.Enter(Generous);var value=MixedCanonicalRational.Parse("-2/3",4);Need(value.Numerator==-2&&value.Denominator==3,"ASCII signed wire");Need(s.Snapshot().FormatCharacters==0,"no culture formatting");}
   finally{CultureInfo.CurrentCulture=old;}
  });
  Console.WriteLine($"{count}/{count} canonical rational tests passed");
 }
}
