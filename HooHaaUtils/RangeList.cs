using System;
using System.Collections.Generic;

namespace HootileriHaa
{
	[Serializable]
	public class RangeList<T, Key> where Key : IComparable
	{

		//Nodo class
		[Serializable]
		internal class Nodo{
			#region Nodo Declarations
			public Key[] llaves;
			public Nodo[] hijos;
			public Nodo padre = null;
			public static int grado;
			public int n;
			#endregion

			#region Nodo Indexer

			public Nodo this[Key key]{
				get{
					for (int i=0; i<n; ++i)
						if (key.CompareTo (llaves [i]) < 0)
							return hijos [i];
					return hijos [n];
				}
			}
			#endregion

			#region Nodo Constructors & Destructor
			public Nodo(Key c, Nodo papi) {
				llaves = new Key[grado];
				llaves[0] = c;
				n = 1;
				hijos = new Nodo[grado+1];
				padre = papi;
			}

			public Nodo(Nodo hijo){
				llaves = new Key[grado];
				hijos = new Nodo[grado+1];
				hijos[0] = hijo;
				hijos[0].padre = this;
			}

			public void Clear(){
				if (this is Hoja)
					return;
				for(int i=0; i<=n; ++i){
					hijos [i].Clear ();
					hijos [i] = null;
				}
			}
			#endregion
			//operator=, vaciar
			#region Nodo Addin' & Removin'
			public bool Add(Key key, Nodo nodo){
				Key kaux;
				Nodo naux;
				if (n == 0 || llaves [n - 1].CompareTo (key) < 0) {
					llaves [n] = key;
					hijos [++n] = nodo;
					return true;
				}
				for (int i=0; i<n; ++i) {
					if (llaves [i].CompareTo (key) > 0) {
						hijos [n + 1] = hijos [i + 1];
						hijos [i + 1] = nodo;
						llaves [n] = llaves [i];
						llaves [i] = key;
						for (int j=n++ -1; j>i; --j) {
							kaux = llaves [j];
							llaves [j] = llaves [j + 1];
							llaves [j + 1] = kaux;
							naux = hijos [j + 1];
							hijos [j + 1] = hijos [j + 2];
							hijos [j + 2] = naux;
						}
						break;
					} else if (llaves [i].CompareTo (key) == 0)
						return false;
				}
				return true;
			}

			public virtual bool Remove(Key key){
				int i=0;
				for (; i < n; ++i)
					if(llaves[i].CompareTo (key) == 0) break;
				if(i == n) return false;

				for (int j = i; j < n-1; ++j){
					hijos[j+1] = hijos[j+2];
					llaves[j] = llaves[j+1];
				}
				--n;
				return true;
			}

			public virtual void Split(){
				int mitad = (grado-1)/2;
				Nodo der = new Nodo(llaves[mitad+1], padre);
				n = mitad;

				for(int i=mitad+2; i<grado; ++i){
					der.llaves[der.n] = llaves[i];
					hijos[i].padre = der;
					der.hijos[der.n++] = hijos[i];
				}
				hijos[grado].padre = der;
				der.hijos[der.n] = hijos[grado];
				padre.Add(llaves[mitad], der);
			}

			public virtual void Merge(){
				Nodo hermano;

				int i=0;
				for(; padre.hijos[i] != this; ++i);
				if(i==0){
					hermano = padre.hijos[i+1];
					llaves[n] = padre.llaves[i];
					for(int j=0; j<hermano.n; ++j){
						hijos[n] = hermano.hijos[j];
						hijos[n].padre = this;
					}
					hijos[n] = hermano.hijos[hermano.n];
					hijos[n].padre = this;
				}else{
					hermano = padre.hijos[i-1];
					hermano.llaves[hermano.n++] = padre.llaves[i-1];
					hermano.hijos[hermano.n] = hijos[0];
					hermano.hijos[hermano.n].padre = hermano;
					for(int j=0; j<n; ++j){
						hermano.llaves[hermano.n++] = llaves[j];
						hermano.hijos[hermano.n] = hijos[j+1];
						hermano.hijos[hermano.n].padre = hermano;
					}
				--i;
				}
				for (int j = i; j < padre.n-1; ++j){
					padre.hijos[j+1] = padre.hijos[j+2];
					padre.llaves[j] = padre.llaves[j+1];
				}
				--padre.n;
			}

			public virtual bool PedirPrestado(){
				Nodo hermano;
				int i=0;

				for(; padre.hijos[i] != this; ++i);
				if(i==0 || (i!=padre.n && padre.hijos[i+1].n > padre.hijos[i-1].n)){
					hermano = padre.hijos[i+1];
					if(hermano.n == (grado-1)/2) return false;

					llaves[n++] = padre.llaves[i];
					hijos[n] = hermano.hijos[0];
					hijos[n].padre = this;
					padre.llaves[i] = hermano.llaves[0];
					for(int j=0; j<hermano.n -1; ++j){
						hermano.llaves[j] = hermano.llaves[j+1];
						hermano.hijos[j] = hermano.hijos[j+1];
					}
					hermano.hijos[hermano.n-1] = hermano.hijos[hermano.n];
					--hermano.n;
				}else{
					hermano = padre.hijos[i-1];
					if(hermano.n == (grado-1)/2) return false;

						hijos[n+1] = hijos[n];
						for(int j=n++; j>0; --j){
							llaves[j] = llaves[j-1];
							hijos[j] = hijos[j-1];
						}
						llaves[0] = padre.llaves[i-1];
						hijos[0] = hermano.hijos[hermano.n];
						hijos[0].padre = this;
						padre.llaves[i-1] = hermano.llaves[--hermano.n];
				}
				return true;
			}
		}

		//Hoja

		internal class Hoja : Nodo{
			#region Hoja Declarations
			public T[] infos;
			#endregion

			#region Hoja Constructor
			public Hoja(T i, Key c, Nodo papi):base(c, papi){
				infos = new T[grado];
				infos[0] = i;
				hijos = new Nodo[1];
				hijos[0] = null;
			}
			#endregion

			#region Hoja Addin' & Removin'
			public bool Add(Key key, T t){
				T aux;
				Key kaux;
				if(n==0 || llaves[n-1].CompareTo (key) < 0){
					llaves[n] = key;
					infos[n++] = t;
					return true;
				}
				for(int i=0; i<n; ++i){
					if(llaves[i].CompareTo (key) > 0){
						infos[n] = infos[i];
						infos[i] = t;
						llaves[n] = llaves[i];
						llaves[i] = key;
						for(int j=n++ -1; j>i; --j){
							aux = infos[j];
							infos[j] = infos[j+1];
							infos[j+1] = aux;
							kaux = llaves[j];
							llaves[j] = llaves[j+1];
							llaves[j+1] = kaux;
						}
						break;
					}else if(llaves[i].CompareTo (key) == 0) return false;
				}
				return true;
			}

			public override bool Remove (Key key)
			{
				int i=0;
				for (; i < n; ++i)
					if(llaves[i].CompareTo (key) == 0) break;
				if(i == n) return false;

				for (int j = i; j < n-1; ++j){
					infos[j] = infos[j+1];
					llaves[j] = llaves[j+1];
				}
				--n;
				return true;
			}

			public override void Split ()
			{
				int mitad = grado/2;
				Hoja der = new Hoja(c: llaves[mitad], i: infos[mitad], papi: padre);
				n = mitad;

				for(int i=mitad+1; i<grado; ++i){
					der.llaves[der.n] = llaves[i];
					der.infos[der.n++] = infos[i];
				}
				//lista enlazada b+
				der.hijos[0] = hijos[0];
				hijos[0] = der;
				padre.Add(llaves[mitad], der);

			}

			public override void Merge ()
			{
				Hoja hermano;

				int i=0;
				for(; padre.hijos[i] != this; ++i);
				if(i==0){
					hermano = (Hoja)padre.hijos[i+1];

					for(int j=0; j<hermano.n; ++j){
						llaves[n] = hermano.llaves[j];
						infos[n++] = hermano.infos[j];
					}
					hijos[0] = hermano.hijos[0];

				}else{
					hermano = (Hoja)padre.hijos[i-1];

					for(int j=0; j<n; ++j){
						hermano.llaves[hermano.n] = llaves[j];
						hermano.infos[hermano.n++] = infos[j];
					}
					hermano.hijos[0] = hijos[0];
					--i;
				}
				for(; i<padre.n-1; ++i){
					padre.llaves[i] = padre.llaves[i+1];
					padre.hijos[i+1] = padre.hijos[i+2];
				}
				--padre.n;
			}

			public override bool PedirPrestado ()
			{
				Hoja hermano;
				int i=0;

				for(; padre.hijos[i] != this; ++i);
				if(i==0 || (i!=padre.n && padre.hijos[i+1].n > padre.hijos[i-1].n)){
					hermano = (Hoja)padre.hijos[i+1];

					if(hermano.n == (grado-1)/2) return false;
					llaves[n] = hermano.llaves[0];
					infos[n++] = hermano.infos[0];
					padre.llaves[i] = hermano.llaves[1];
					for(int j=0; j<hermano.n -1; ++j){
						hermano.llaves[j] = hermano.llaves[j];
						hermano.infos[j] = hermano.infos[j+1];
					}
					--hermano.n;
				}else{
					hermano = (Hoja)padre.hijos[i-1];

					if(hermano.n == (grado-1)/2) return false;
					for(int j=n++; j>0; --j){
						llaves[j] = llaves[j-1];
						infos[j] = infos[j-1];
					}
					infos[0] = hermano.infos[--hermano.n];
					llaves[0] = hermano.llaves[hermano.n];
					padre.llaves[i-1] = llaves[0];
				}
				return true;
			}
			#endregion
		}

		//Iterator

		public class RangeListIterator{

			private Hoja currentHoja;
			private int currentIndex;
			private Key border;

			public Key CurrentKey{
				get{ return currentHoja.llaves [currentIndex]; }
			}

			public T CurrentElement{
				get{ return currentHoja.infos [currentIndex]; }
			}

			internal RangeListIterator(){}

			internal RangeListIterator Initialize(Hoja h, int i, Key b){
				currentHoja = h;
				currentIndex = i-1;
				border = b;
				return this;
			}

			/*public bool HasNext(){
				if (currentHoja.n < currentIndex + 1)
					return currentHoja.hijos [0].llaves [0].CompareTo (border) <= 0;
				return currentHoja.llaves [currentIndex + 1].CompareTo (border) <= 0;
			}*/

			public bool Next(){
				if (currentHoja == null)
					return false;
				if (currentHoja.n <= currentIndex + 1) {
					currentHoja = (Hoja)currentHoja.hijos [0];
					if (currentHoja == null)
						return false;
					currentIndex = -1;
				}if (currentHoja.llaves [currentIndex + 1].CompareTo (border) > 0) {
					currentHoja = null;
					return false;
				}
				++currentIndex;
				return true;
			}

		}

		//RangeList

		#region Declarations
		private Nodo raiz = null;
		private Hoja hojaMasIzquierda = null;
		private RangeListIterator iterator = new RangeListIterator();
		private readonly int grado;
		private readonly int min;
		private int size;
		#endregion

		#region Properties & Indexer
		public int Size{
			get{ return size; }
		}

		public T this[Key llave]{
			get{
				Hoja hoja = HojaWithKey (llave);
				if (hoja != null)
					for (int i=0; i<hoja.n; ++i)
						if (hoja.llaves [i].CompareTo (llave) == 0)
							return hoja.infos [i];
				return default(T);
			}
		}

		public RangeListIterator this[Key a, Key b]{
			get{
				if (raiz == null)
					return iterator;
				Nodo aux = raiz;
				while (!(aux is Hoja))
					aux = aux [a];
				int i = 0;
				for (; aux.llaves[i].CompareTo (a) < 0; ++i);
				return iterator.Initialize((Hoja)aux, i, b);
			}
		}

		#region Constructor
		public RangeList (int g = 5)
		{
			grado = g;
			min = (g - 1) / 2;
			Nodo.grado = g;
		}
		#endregion

		#endregion Public Methods
		public bool Add(T valor, Key llave){
			Nodo aux = raiz;
			while(aux != null){
				if(aux is Hoja){
					if(((Hoja)aux).Add(llave, valor)){
						Rebalance(aux);
						++size;
						return true;
					}else return false;
				}else aux = aux[llave];
			}
			raiz = new Hoja(c: llave, i: valor, papi: null);
			hojaMasIzquierda = (Hoja)raiz;
			++size;
			return true;
		}

		public bool Remove(Key llave){
			Nodo aux = raiz;
			while(aux != null)
				if(!(aux is Hoja)) aux = aux[llave];
				else if(aux.Remove(llave)){
					Rebalance(aux);
					--size;
					return true;
				}else return false;
			return false;
		}

		public bool HasKey(Key llave){
			return HojaWithKey (llave) != null;
		}

		public override string ToString ()
		{
			Hoja aux = hojaMasIzquierda;
			string cout = "[";
			while(aux != null){
				for(int i=0; i<aux.n; ++i)
					cout += aux.infos[i].ToString() + ",";
				aux = (Hoja)aux.hijos[0];
			}
			cout += "\b]";
			return cout;
		}

		public void Clear(){
			if (raiz == null)
				return;
			raiz.Clear ();
			raiz = null;
			size = 0;
		}
		#endregion

		#region Private Methods
		private Hoja HojaWithKey(Key llave){
			if (raiz == null)
				return null;
			Nodo actual = raiz;
			while(!(actual is Hoja))
				actual = actual[llave];
			for(int i=0; i<actual.n; ++i)
				if(actual.llaves[i].CompareTo (llave) == 0) return (Hoja)actual;
			return null;
		}

		private void Rebalance(Nodo r){
			if(r.n == grado){
				//>=MAX
				if(r==raiz){
					new Nodo(r);
					raiz = r.padre;
				}
				r.Split();
				if(r.padre != null) Rebalance(r.padre);
			}else if (r.n < min){
				if(r == raiz){
					if(r.n == 0){
						raiz = r.hijos[0];
					}
					return;
				}
				if(!r.PedirPrestado()){
					r.Merge();
					Rebalance(r.padre);
				}
			}
		}
		#endregion

	}
}

