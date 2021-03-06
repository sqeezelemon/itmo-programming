# Лабораторная работа 6: Programming at compile-time

## Задача
1. Вам требуется реализовать возможность вычисления значение в
точке многочлена с целочисленными коэффициентами. Точка
также целочисленная. Все вычисления должны происходить в
момент компиляции.
2. Перегрузить оператор вывода для многочлена из пункта 1.
3. Полученный код протестировать с помощью [GoogleTest Framework](https://google.github.io/googletest/)

## Реализация
Помимо всего выше, с помощью дебрей рекурсии реализованы следующие операторы и функции:
* `operator==`, `operator!=` - Сравнение полиномов
* `operator+`, `operator` - Сложение и вычитание между полиномами
* typecast из полинома одной степени в полином другой степени
* `maxpow()` - Возвращает самую большую ненулевую степень

### Полином спроектирован следующим образом:
```
template <int Power, class T = int>
class polynomial {
private:
	T* coeffs[Power+1];
	// ce_... функции для constexpr рекурсии
public:
	Инициализаторы, операторы и некоторые функции
}
```
### Тестирование
Для тестов достаточно запустить файл `test.sh`