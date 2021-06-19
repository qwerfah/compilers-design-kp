class A {
	def f(): Int = ???
	val o = 'l'

	private def f(b: Double): A = ???
	protected def f(b: Int): Double = {
		val y = f(b)
		val c = f(y).f() == 'f'
		return y ++ b
	}
}

abstract class B extends A {
	def g(j: Int): Double = f(j) 
	def r(): Unit = {
		val t = g(100)
	}
}