using System;
using System.Collections.Generic;
using System.Text;

namespace TimeLords
{
    public class Material
    {
        public static Material air = new Material("Air",
            new List<Molecule> { Molecule.H2O });

        public static Material rock = new Material("Granite",
            new List<Molecule> { Molecule.FeMgSiO4 });
        

        public float boilPoint {get;set;}
		public float meltPoint {get;set;}
		public float density {get;set;}
        public List<Molecule> moleculeList {get;set;}
        public string name {get;set;}
		
		public bool isDiggable
		{
			get
			{
				if (density > 2f && density < 7f) //Talc is 2.75, steel is 7.75
					return true;
				else
					return false;
			}
			
			set
			{
				if (value == true)
					density = 3f;
				else
					density = 1f;
			}
		} //Whether the material can be dug through
		public bool isTransparent
		{
			get {return (density < 1.5f);}
			set {}		
		}

		public Material():this("missingmaterial"){}
		public Material(string name):this(name, new List<Molecule>()){}
		public Material(string name, List<Molecule> moleculeList):this(name, moleculeList, 1f, 1f, 1f){}
        public Material(string name, List<Molecule> moleculeList, float density,
            float boilPoint, float meltPoint)
        {
            this.name = name;
            this.moleculeList = moleculeList;
            this.density = density;
            this.boilPoint = boilPoint;
            this.meltPoint = meltPoint;
        }
		public Material(Material m)
		{
			this.boilPoint = m.boilPoint;
			this.meltPoint = m.meltPoint;
			this.density = m.density;
			this.moleculeList = m.moleculeList;
			this.name = m.name;
		}

        //String representation of an atom should be its name
        public override string ToString()
        {
            return name;
        }
    } //Made up of one or several molecules
}
