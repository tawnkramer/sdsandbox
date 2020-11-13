cote=85;
hauteur=160
r=cote*sqrt(3)/2
print (r); 

translate([0,0,0])cylinder(r=160,h=1,$fn=6);
difference ()
{
hull()
{

translate([0,0,0])cylinder(r=150,h=1,$fn=6);
translate([0,0,200])cylinder(r=50,h=1,$fn=6);

}
hull(){

translate([0,100,180])rotate([90,0,0]) cylinder(r=10,h=250);
translate([0,100,220])rotate([90,0,0]) cylinder(r=10,h=250);
    
}

}
