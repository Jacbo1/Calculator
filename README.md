This calculator uses a custom fraction class for numbers which allows it to accurately store and perform arithmetic on huge or miniscule numbers with no precision loss for most things. Exceptions are sin, cos, tan, asin, acos, atan, and ^ (only fractions with extremely large numerators or denominators will lose precision). It also allows outputs to be displayed in extreme accuracy. For example, you can output the square root of 2 that is accurate to 100 decimal places (this precision is controlled by the readonly `POW_EPSILON` variable).  
  
This calculator supports order of operations, parentheses, and vectors. Supported operators are +, -, \*, /, %, ^, x (cross product), and . (dot product). Vectors are formatted like <1,2,3> but can have equations in them too. Multiplying 2 vectors with \* will produce a new vector as <x1\*x2,y1\*y2,z1\*z2>.  
Supported functions that do not require parentheses are sin, cos, tan, asin, acos, atan, deg (converts radians to degrees, same as \* 180 / pi), rad (converts degrees to radians, same as \* pi / 180), abs, floor, ceil, sqrt (square root), sign (returns 1 if positive, 0 if 0, -1 if negative), and sigfig4 (very niche, added it because I needed numbers with only 4 significant figures for something).  
Supported functions that do require parentheses are min, max, clamp, log, round, sum (summation), prod (product/pi notation), getx, gety, getz, length, norm, and atan2.
Supported constants are pi and e.  
Variables are also supported.  
Vectors work with all operators and functions.  
**More detailed information can be found on the [wiki](https://github.com/Jacbo1/Calculator/wiki)**  
  
To use variables, type it in the format of `a=1+2`. Each line is a new equation. Variable names can overwrite existing operator and constant names. Useful for if you want to use the variable name "x" and won't be needing to do cross product.  
  
Final answers resulting from the assignment of a variable are never rounded or put in scientific notation. E.g. if the final line is `x=12^-10` then the displayed answer will be `0.0000000000161505582889845721` but if the final line is `12^-10` the displayed answer will be `1.6150558289E-11`.  
Numbers are rounded to 10 decimal places. Numbers will be put into scientific notation when their absolute value is less than 0.00001 (i.e. at least 5 0s before the decimal point and rest of the number). This is controled by the `DEC_DIGIT_DISPLAY` constant in Formula.cs  

In the settings drop down menu, the work output can be toggled and "Only 1 instance" mode can also be enabled where running the program will essentially toggle it. In this mode, when running the program and it finds no other instances already running, it will open. If it does find other instances, it will close them and not open. This is useful for replacing the default calculator using something like the AutoHotkey script below.  

An AutoHotkey script can be used to replace the windows calculator with this when you press your calculator key on your keyboard (if you have one). Recommended to enable "Only 1 instance" mode so pressing the key toggles the calculator.
```
#NoEnv
SendMode Input
SetWorkingDir %A_ScriptDir%

Launch_App2::
    run PathToCalculator\Calculator.exe
    return
```
![Basic](Screenshots/basic%203.png)  
  
![Vectors](Screenshots/vectors%202.png)  
  
![Variables](Screenshots/decimal%20and%20var%205.png)

![Work toggled off](Screenshots/variables%202.png)
  
I give full permission to use any code from this as long as you don't claim you wrote it. Attribution is not needed.