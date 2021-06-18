class A {
	def f(): Unit = ???
	private def f(b: Double): Double = ???
	protected def f(b: Int): Double = {
		val y = f(b)
		val c = y ** f(y)
		return y ++ b
	}
}

abstract class B extends A {
	def g(j: Int): Double = f(j) 
	def r(): Unit = {
		val t = g(100)
	}
}
