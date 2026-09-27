"use strict";

// Shared strict ASCII/compact/duplicate-free depth16 parser. Mechanically
// extracted from the uncalled A68 verifier; no phase/replay is activated.
const parseCanonicalWire = (bytes, limits) => {
  const need=(ok,label)=>{if(!ok)throw new Error("Phase627 DAG wire: "+label);};
  need(Number.isSafeInteger(limits.graphBytes)&&limits.graphBytes>0,"positive safe byte limit");
  need(Buffer.isBuffer(bytes)&&bytes.length>1&&bytes.length<=limits.graphBytes,"pre-parse byte ceiling");
  need(bytes[bytes.length-1]===10&&bytes[bytes.length-2]!==10,"single final LF");
  // The frozen schema, rational strings, names and repository paths are all
  // ASCII. Refuse other encodings instead of silently replacing invalid
  // UTF-8, accepting lone surrogates, or depending on cross-runtime escaping.
  for(const b of bytes)need(b>=0x20&&b<=0x7e||b===10,"ASCII wire domain");
  const text=bytes.toString("ascii");let position=0;
  const string=()=>{
    need(text[position]==='"',"string token");const start=position++;
    while(position<text.length&&text[position]!=='"'){
      if(text[position]==="\\")position++;
      position++;
    }
    need(position<text.length,"terminated string");position++;
    const value=JSON.parse(text.slice(start,position));
    need(typeof value==="string"&&/^[\x20-\x7e]*$/.test(value),"ASCII string domain");return value;
  };
  const value=depth=>{
    need(depth<=16,"maximum depth16");const token=text[position];
    if(token==='"'){string();return;}
    if(token==="{"){
      position++;const seen=new Set();if(text[position]==="}"){position++;return;}
      while(true){const name=string();need(!seen.has(name),"duplicate property");seen.add(name);need(text[position++]===':',"property colon");value(depth+1);const next=text[position++];if(next==='}')return;need(next===',',"object separator");}
    }
    if(token==="["){
      position++;if(text[position]==="]"){position++;return;}
      while(true){value(depth+1);const next=text[position++];if(next===']')return;need(next===',',"array separator");}
    }
    for(const literal of ["true","false","null"])if(text.startsWith(literal,position)){position+=literal.length;return;}
    const start=position;if(text[position]==="-")position++;
    while(position<text.length&&text[position]>="0"&&text[position]<="9")position++;
    const number=text.slice(start,position);
    need(/^-?(0|[1-9][0-9]*)$/.test(number)&&number!=="-0"&&Number.isSafeInteger(Number(number)),"canonical safe integer");
  };
  value(1);need(position===text.length-1,"no whitespace/trailing tokens");
  const graph=JSON.parse(text);
  need(JSON.stringify(graph)+"\n"===text,"canonical compact ordered JSON");return graph;
};

module.exports = { parseCanonicalWire };
