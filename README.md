# Описание
**VariableTools** - это плагин для бота [Astral](https://www.neverwinter-bot.com/forums/index.php) к MMORPG ["Neverwinter Online"](https://www.arcgames.com/en/games/neverwinter/news).
Он предназначен для работы с переменными в сценариях [Quester'a](https://www.neverwinter-bot.com/forums/viewtopic.php?f=150&t=7892).

Ключевыми объектами в данном плагине являются [**переменная**](#ref-Varibale) и [**арифметическое выражение**](#ref-Expression).


---

# <a name="ref-Variable"></a>**Переменная**

Переменная - это действительное число, идентифицируемое комбинацией трёх признаков: 
- <a name="ref-Name"></a>**Name** - текстовое имя переменной (Simple или Regex).  
  В имени переменной допускаются любые русские и латинские буквы, цифры и символ ``_``.  
  При этом имя переменой не должно совпадать с именами функторов, используемых в [*EXPRESSION*](#ref-Expression);
- <a name="ref-AccountScope"></a>**AccountScope** - переключатель области видимости для персонажей:
    + *Global* : переменная видима всем персонажам независимо от аккаунта;
    + *Account* : переменная видима всем персонажам конкретного аккаунта, который был загружен при её инициализации;
    + *Character* : переменная видима единственному персонажу, который был активен при её инициализации.
- <a name="ref-ProfileScope"></a>**ProfileScope** - переключатель области видимости относительно профиля:
    + *Common* : область видимости переменной не ограничена;
    + *Profile* : область видимости переменной ограничена конкретным Quester-профилем, в котором она была инициализирована.  

Переменные сохраняются во внутренней коллекции со значениями, полученными в результате выполнения Quester-профиля. По этой причини рекомендую нужные вам переменные инициализировать, чтобы при повторном запуске скрипта не получить ошибок из-за старых значений.

<a name="ref-Save"></a>Переменные, с флагом **Save**, при завершении работы Астрала сохраняются в файл и загружаются при следующем запуске бота. Таким образом обеспечивается сохранение значений переменных между игровыми сессиями.

---

# <a name="ref-Expression"></a>**Арифметическое выражение (EXPRESSION)**

Арифметическое выражение состоит из следующих элементов:
- **Числовые константы** (целые числа или десятичные дроби).  
  В качестве разделителя дробной части можно использовать ``.`` и ``,``;
- [**Переменные**](#ref-Variable), область видимости которых задается в виде перечисления идентификаторов [*AccountScope*](#ref-AccountScope) и [*ProfileScope*](#ref-ProfileScope) в квадратных скобках сразу после имени переменной.  
  При отсутствии соответствующего идентификатора переменная распознается как ``[Global, Common]``.  
  Запись ``SomeVar`` эквивалентна ``SomeVar [Global,Common]``;  
  Запись ``SomeVar[Character]`` эквивалентна ``SomeVar [Character, Common]``;  
  Запись ``SomeVar[Profile]`` эквивалентна ``SomeVar [Global, Profile]``;  
  Запись ``SomeVar[Account, Common]`` - полное определение переменной.
- Простые **арифметические операторы**: ``+ - * /``;
- Операцию **взятия остатка от деления**: ``%``;
- Группирующие **скобки**: ``(`` и ``)``;
- [**Разрешенные функции**](#ref-Functions);
- [**Функции времени**](#ref-Functions);
  
Пробелы в выражении игнорируются, однако, пробел не может разделять две части имени переменной или функтора:  
``My_Var   iable_001`` - некорректное выражение

Не инициализированные переменные, упомянутые в ___EXPRESSION___,  считаются равными нулю.

Результат некорректного выражения ___EXPRESSION___  на этапе выполнения равен нулю.

Результатом деления на '0' будет максимально возможное число для типа double. Что-то в районе 1.7976931348623157E+308.

Выражение ***EXPRESSION***  парсится (т.е. "разбирается" и проверяется) единственный раз во время инициализации, в результате чего строится Абстрактное синтаксическое дерево (АСТ), используемое для вычисления во время выполнения скрипта. 
<!-- Я не ожидал, что механизм обработки исключений в .Net такой громоздкий, поэтому текущая реализация требует значительного времени для парсинга ___Equation___  (до нескольких секунд на длинное выражение)... Буду искать варианты оптимизации. 
На этапе выполнения скрипта время вычисления такого выражения было пренебрежимо мало (мои средства мониторинга показывали 0~1 мс). -->

---

## <a name="ref-SetVariable"></a>**Команда SetVariable**

Данная команда присваивает заданной переменной, результат арифметического выражения, заданного опцией [*Equation*](#ref-Equation).  

|Опция|Описание|
|:---:|:-------|
|***Variable***|Комплексный Идентификатор [переменной](#ref-Variable)|
|***Equation***|[Арифметическое выражение](ref-Expression), результат которого присваивается переменной |
|___Save___| Флаг, указывающий на необходимость сохранения переменной в файл при завершении работы Астрала.<br/>- **True** - устанавливает [флаг](#ref-Save) сохранения в файл;<br/>- **False** - снимает [флаг](#ref-Save) сохранения;<br/>- **NotChange** - значение [флага](#ref-Save) не изменяется.


В команде **SetVariable** в выражении _Equation_ можно использовать переменную, которой присваивается значение. В этом случае сначала будет вычислено значение выражения _Equation_ (считано старое значение переменной), а затем переменной будет присвоен полученный результат. 
Например:
```Num := Num + 99```

__Внимание__: изменение имени, и идентификаторов области видимости *AccountScope*, *ProfileScope* в команде **SetVariable** не изменяют эти признаки для переменной в коллекции. Таким образом вы просто определите "новую" переменную с другим именем и(или) областью видимости.

<!-- ```Это_имя_переменной_1
This_is_тоже_correct_VARIABLE_name``` -->

## <a name="ref-DeleteVariable"></a>**Команда DeleteVariable**
Выполнение команды удаляет из внутренней коллекции заданную переменную.  

|Опция|Описание|
|:---:|:-------|
|***Variable***|Комплексный Идентификатор удаляемой [переменной](#ref-Variable)|

Удаленная переменная не будет сохранена в файл.

---

## <a name="ref-CheсkEquations"></a> **Условие CheсkEquations**

Условие сравнивает результат арифметического выражения [Equation1](#ref-Equation) с результатом арифметического выражения [Equation2](#ref-Equation).
Тип сравнения задается полем ___Sign___ и может принимать значения:
- **Inferior** : Условие истинно, если ``Equation1`` **меньше** ``Equation2``;
- **Superior** : Условие истинно, если ``Equation1`` **больше** ``Equation2``;
- **Equal** : Условие истинно, если ``Equation1`` **равно** ``Equation2``;
- **NotEqual** : Условие истинно, если ``Equation1``  **НЕ равно** ``Equation2``.

## **Условие CheckVariable**

Данное условие позволяет проверить наличие(отсутствие) [переменной](#ref-Variable) в коллекции.
<!-- Переменная задается комбинацией признаков
- ___Name___ - текстовое имя переменной (Simple или Regex)
- ___AccountScope___ - область видимости для всех/аккаунта/персонажа
- ___ProfileScope___ - область видимости для всех/профиля
Тип имени переменной определяется опцией ___NameType___ (Simple или Regex) -->

Доступны две проверки:
- ___VariablePresenceCheck___ : Условие истинно, если во внутренней коллекции содержится заданная переменная;
- ___VariablesCountCheck___ : производит подсчет количества переменных, соответствующих комбинации признаков (имя, область видимости), и сравнивает его с заданным числовым значением.

---

## <a name="ref-Functions"></a>**Функции**
- Функцию *ItemCount(ItemId)*, вычисляющую количество предметов, заданных полным идентификатором _ItemId_.  
  В начале и конце идентификатора можно использовать символ подстановки ``*``, заменяющий любое число символов;
- Функцию ``NumericCount(NumericId)``, вычисляющую количество Numeric, заданных полным идентификатором _NumericId_.  
  В начале и конце идентификатора можно использовать символ подстановки ``*``, заменяющий любое число символов;
- Функцию ``Random(Equation)``, генерирующую случайное целое число от нуля до значения выражения _Equation_. 
  Если _Equation_ пустое, тогда максимальное генерируемое значение определяется системными характеристиками. У меня это 2'147'483'647.  
  ```Random(Var_001 * (ItemCount(Gemfood_1)%55.9)) / Var_002```  
  Результат приведенного выражения - частное от деления случайного числа на переменную Var_002.  
  Максимальное возможно значение случайного числа задано произведением:  
  ``Var_001 * (ItemCount(Gemfood_1)%55.9)``.  
  
## <a name="ref-DateTimeFunctions"></a>**Функции времени**
Любая переменная представляет действительное число, которое может быть интерпретировано как дата-время в формате [*OLE Automation Date*](https://docs.microsoft.com/ru-ru/dotnet/api/system.datetime.tooadate?view=netframework-4.8#-----------).  

Полночь (0 час. 0 мин) 30.12.1899 года соответствует ``0,0``.  
Целая единица соответствует суткам, а дробная часть - времени (час = 1/24; минута = 1/1440; секунда = 1/86400).  

Для удобства работы с переменными как с датами определены функторы:
``Now()`` - возвращает текущую дату и время;
``Days(EXPRESSION)`` - количество дней, заданных EXPRESSION
``Hours(EXPRESSION)`` - количество часов, 
``Minutes(EXPRESSION)`` - количество минут, 
``Seconds(EXPRESSION)`` - количество секунд, 
Где ``EXPRESSION`` - любое допустимое [арифметическое выражение](#ref-Expression)

Например, день недели можно вычислить по формуле:```Days(Now() - 2) % 7```0 - понедельник
1 - вторник
...
6 - воскресенье
