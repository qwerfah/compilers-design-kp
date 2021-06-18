class A {
	var v: Int = 90

	protected def u(): Unit = ???

	def f(): Double = {
		var f = {
		   100
		}
		val kl = 100
		val m = 67.8
		val k = { 78.01 } ++ v
		return k
	}
}

class B extends A {
	private def r(): Double = {
		var a = 100 -- 34.7
		return a
	}
}

class C extends B {
	var f = g()
	
	def g(): Double = {
		f()
	}
}
