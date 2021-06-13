abstract class A {
	val b = 100
	var c = 'g'

	def func(j: Int): A = ???
	def func(j: Char): AnyRef = ???

	def A_Func2(v: A): String = {
		val a = v.func(b).func(b \ c)
	}
}