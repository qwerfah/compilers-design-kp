class A(a: Int, var b: Double, val c: Boolean, private var d: Int) {
	def A_Func1(a: Int, b: Double): Unit = ???
	def A_Func2(): F = ???
}

class B {
	class F extends B

	val B_Var1: Int
	var B_Var2: Double
	private var B_var3: Boolean
	protected var B_var4: Any

	def B_Func1() : Unit = ???
	def B_Func2(first: A, second: A) : A = {
		a = B_Func1().B_Func2().B_Func2()

	}
}

class C extends A {
}

class D extends B {
}