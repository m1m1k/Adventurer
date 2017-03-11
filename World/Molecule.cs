using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeLords
{
    [Serializable]
    public class Molecule
    {
        public static readonly Molecule H2O = new Molecule("H2O",
            Multi(Combine(Atom.O), Atom.H, 2)
        );
        // http://www.eeescience.utoledo.edu/Faculty/Krantz/Hazards/Hazards.Chap_04a.minerals_&_rocks.pdf
        //Ferromagnesian
        // Two most abundant rocks on earth (most of earth's mantle and oceanic lithosphere)

        public static readonly Molecule FeMgSiO4 = new Molecule("FeMgSiO4",
            Multi(Combine(Atom.Fe, Atom.Mg, Atom.Si), Atom.O, 4)
        );
        //Non-ferromagnesian
        public static readonly Molecule CaSiO4 = new Molecule("CaSiO4",
            Multi(Combine(Atom.Ca, Atom.Si), Atom.O, 4)
        );
        public static readonly Molecule NaSiO4 = new Molecule("NaSiO4",
            Multi(Combine(Atom.Na, Atom.Si), Atom.O, 4)
        );
        public static readonly Molecule KSiO4 = new Molecule("KSiO4",
            Multi(Combine(Atom.K, Atom.Si), Atom.O, 4)
        );

        

        public string name {get;set;}
		public float meltPoint {get;set;}
		public float boilPoint {get;set;}
        public List<Atom> atomList {get;set;}

		public Molecule():this("missingmo"){}
		public Molecule(string name):this(name, new List<Atom>()){}
		public Molecule(string name, List<Atom> atomList):this(name, 0f, 100f, atomList){}
        public Molecule(string name, float meltPoint, float boilPoint, List<Atom> atomList)
        {
            this.name = name;
            this.meltPoint = meltPoint;
            this.boilPoint = boilPoint;
            this.atomList = atomList;
        }
		public Molecule(Molecule m)
		{
			this.name = m.name;
			this.meltPoint = m.meltPoint;
			this.boilPoint = m.boilPoint;
			this.atomList = m.atomList;
		}

        public static List<Atom> Multi(List<Atom> list, Atom atom, int repeats)
        {
            if(list == null)
            {
                list = new List<Atom>();
            }
            return list.Union(Enumerable.Repeat(atom, repeats)).ToList();
        }
        public static List<Atom> Combine(params Atom[] atom)
        {
            var myList = new List<Atom> ();
            foreach(Atom a in atom)
            {
                myList.Add(a);
            }
            return myList;
        }
    } //Made of a bunch of atoms
}
