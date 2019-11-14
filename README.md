# mzConvertToBGMS
This project is meant for generic conversion from mzXML to Biognosys BGMS file format.

I added a working version for mzXML to BGMS as well as demo code for Thermo raw support
(all working but I did not want to share the Thermo API publically here).

Overall, this project is meant as a starting point for others to add more features.
I am personally not capable of contributing much beyond this point since I have a full-time job as well.
However, I will always be available for questions via:

oliver.bernhardt@biognosys.com

Whoever is willing to take over the torch is welcome.
If you have questions about the currently available code structure, let me know.

If you want to add new formats you only need to derive a new class from

```
AScanReader
```
and add it to the "factory" 

```
AScanReader.getScanReader(string file);
```

PLEASE FEEL FREE TO SHARE AND ADD TO THIS PROJECT


Currently, the usage of this converter is fairly straight forward.

#### Required Commands:  
```
-in [input file or folder]
```

#### Optional Commands:  
```
-out [output file or folder]
```

If -out is not provided, it will just place the BGMS file next to the input file under the same name but with .bgms appended.
