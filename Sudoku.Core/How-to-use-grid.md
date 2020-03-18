﻿## Grid Format and Parsing format

标题：**盘面格式和解析为盘面的字符串样式**

If you has known the whole outline of this solution, you want to know how to use grid format. First of all, I will give you some characters you should use.

如果你对这个项目有所了解的话，你肯定想知道数独盘面的输入的具体格式。首先我会给你一个表，陈列的各种字符就是你需要用到的。

### Single line format characters

标题：**单行输出模式下的格式化字符**

| Format characters<br/>格式化字符 | Meanings<br/>意思                                                                  |
| -------------------------------- | ---------------------------------------------------------------------------------- |
| `.` and `0`                      | Placeholder option.<br/>占位符选项。                                                |
| `+`                              | Modifiable values option.<br/>显示可修改的数值选项。                                |
| `!`                              | Modifiable values will be regarded as given ones.<br/>把可修改数值视为提示数的选项。 |
| `:`                              | Candidates-has-been-eliminated option.<br/>显示盘面已删除的候选数的选项。            |
| `#`                              | Intelligent output option.<br/>智能输出选项。                                       |

If you write `grid.ToString("0")`, all empty cells will be replaced by character `'0'`; but if you write `grid.ToString(".")`, empty cells will be shown by `'.'`.

如果你写的是 `grid.ToString("0")` 的话，所有的空格都会使用字符 `'0'` 所占据；而如果你写的是 `grid.ToString(".")` 的话，就会是 `'.'` 字符填充空格了。

If you add modifiable-value option `'+'`, all modifiable values (if exists) will be shown by `+digit` (In default case, modifiable values will not be output). In addition, if you write `'!'` rather than `'+'`, all modifiable values will be treated as given ones, so output will not be with plus symbol (i.e. `digit` rather than `'+digit'`). **Note that you should write either `'+'` or `'!'`**. Both characters written will generate a runtime exception.

如果你使用了可修改数值的选项 `'+'` 的话，所有可修改的数值（如果存在的话）都会以 `'+数字'` 的形式被显示在盘面里（在默认情况下，这些数字都是不会显示出来的）。另外，如果你写的是 `'!'` 而不是 `'+'` 的话，所有可修改的数值都会被当作提示数看待，输出的时候不会带有 `+` 符号。注意**这两个符号不要同时使用**。同时使用它们会产生一个运行时异常。

If you want to show candidates which have been eliminated before, you should add `':'` at the tail of the format string, which means all candidates that have been eliminated before current grid status can be also displayed in the output. However, **you should add this option `':'` at the tail position only**; otherwise, generating a runtime exception.

如果你想展示盘面当前情况下的候选数被删除的样子的话，你可以在整个格式化字符串的末尾添加 `':'` 字符，这表示所有当前盘面下被删除的候选数也会被显示出来。但是，**你只能把这个字符放在整个格式化字符串的末尾**，否则它会生成一个运行时异常。

If you cannot raise a decision, you can try intelligent output option `'#'`, which will be output intelligently.

如果你无法作出决定，你可以使用 `'#'` 字符作为格式化字符串，来表示输出智能化处理（检测到有候选数排除的情况会被显示出来；检测到如果有填入的数字也会被呈现出来）。

For examples, if you write:

比如你这么写代码：

```csharp
grid.ToString("0+");
```

This format `"0+"` means that all empty cells will be shown as digit 0, givens will be shown with digit 1 to 9, and modifiable values will be shown.

这个格式化字符串 `"0+"` 表示你的空格是用 `'0'` 字符表示的，并且还会显示所有可修改的数字；而提示数则直接使用 1 到 9 显示。

And another example:

另外一个例子：

```csharp
grid.ToString(".+:");
```

Output will treat:

输出：

* empty cells as `'.'` character,<br/>空格用 `'.'` 占位；
* modifiable values as `'+digit'`,<br/>可修改数值用 `'+数字'` 形式显示；
* candidates as `':candidateList'`.<br/>候选数使用 `:候选数序列` 形式显示。



All examples are shown at the end of this part.

上面所有解释在最后都会给出例子集，可以对照。



### Multi-line format characters

标题：**多行输出模式下的格式化字符**

If you want to output pencil marked grid (PM grid), you should use options below:

另外，如果你要输出这个题目的候选数盘面的话，你可以使用下面的选项：

| Format characters<br/>格式化字符 | Meanings<br/>意思                                                      |
| -------------------------------- | ---------------------------------------------------------------------- |
| `@`                              | Default PM grid character.<br/>默认的候选数盘面输出的格式化字符。        |
| `0` and `.`                      | Placeholders.<br/>占位符。                                             |
| `:`                              | Candidates option.<br/>输出候选数选项。                                 |
| `*`                              | Simple output option.<br/>普通格线字符输出选项。                        |
| `!`                              | Treat-modifiable-as-given option.<br/>把填入的数字视为提示数的选项。     |

These option are same or similar as normal grid (Susser format) output, so I don't give an introduction about those characters. Learn them from examples at the end of this part. I wanna introduce `'*'` option which is not mentioned above however.

这些选项都和普通盘面输出样式的输出模式差不多，所以我就不给出解释了。你可以在例子集里找到这些东西的详细用法。不过我需要提一下上文没有介绍过的 `'*'` 字符。

By the way, character `'*'` is for simple output. If the format has not followed by this option, the grid outline will be handled subtly. You can find the difference between two outputs:

当然了，字符 `'*'` 用作简单格线的输出。如果你的格式化字符串没有这个选项的话，那么格线看起来就没有那么“圆润”。你可以对比下面两个示例，看看区别。

```
.---------------.---------------.------------------.
| 17   128  78  | 135  <9>  35  | *4*   12378  *6* |
| *9*  128  <3> | <4>  18   *6* | 1278  1278   <5> |
| <5>  <6>  *4* | <2>  138  <7> | <9>   138    13  |
:---------------+---------------+------------------:
| <8>  13   <2> | *6*  *5*  *9* | 17    <4>    137 |
| 13   <7>  <5> | 13   *4*  *8* | <6>   <9>    *2* |
| *6*  <4>  *9* | *7*  123  23  | <5>   13     <8> |
:---------------+---------------+------------------:
| 37   *5*  <1> | <8>  23   <4> | 27    <6>    <9> |
| <2>  89   *6* | 59   *7*  <1> | <3>   58     *4* |
| *4*  389  78  | 359  <6>  235 | 1278  12578  17  |
'---------------'---------------'------------------'

+---------------+---------------+------------------+
| 17   128  78  | 135  <9>  35  | *4*   12378  *6* |
| *9*  128  <3> | <4>  18   *6* | 1278  1278   <5> |
| <5>  <6>  *4* | <2>  138  <7> | <9>   138    13  |
+---------------+---------------+------------------+
| <8>  13   <2> | *6*  *5*  *9* | 17    <4>    137 |
| 13   <7>  <5> | 13   *4*  *8* | <6>   <9>    *2* |
| *6*  <4>  *9* | *7*  123  23  | <5>   13     <8> |
+---------------+---------------+------------------+
| 37   *5*  <1> | <8>  23   <4> | 27    <6>    <9> |
| <2>  89   *6* | 59   *7*  <1> | <3>   58     *4* |
| *4*  389  78  | 359  <6>  235 | 1278  12578  17  |
+---------------+---------------+------------------+
```

Multi-line output environment will be more relaxed when ordering different options than single line output one.

多行输出环境下的格式化字符顺序要在单行输出环境下的要求更宽松一些。

Note that in multi-line output environment, placeholder characters `'0'` or `'.'` cannot appear with candidates option `':'` together, because placeholders may not appear when outputting all candidates.

唯一需要注意的地方是，占位符 `'0'` 和 `'.'` 不允许和 `':'` 一起出现，因为需要输出候选数情况时，是不可能出现占位符的。



### Examples

标题：**示例**

![](../pic/P1.png)

```
Format（格式）:
"0"

Output（输出结果）:
800190030190007600002000000000301504000050000704906000000000900008700051040069007

---

Format（格式）:
"@:"

Output（输出结果）:
.---------------------.--------------------.----------------------.
| <8>   567     567   | <1>   <9>    245   | 247    <3>     25    |
| <1>   <9>     35    | 2458  2348   <7>   | <6>    248     258   |
| 3456  3567    <2>   | 4568  348    3458  | 1478   14789   589   |
:---------------------+--------------------+----------------------:
| 269   268     69    | <3>   278    <1>   | <5>    26789   <4>   |
| 2369  12368   1369  | 248   <5>    248   | 12378  126789  23689 |
| <7>   12358   <4>   | <9>   28     <6>   | 1238   128     238   |
:---------------------+--------------------+----------------------:
| 2356  123567  13567 | 2458  12348  23458 | <9>    2468    2368  |
| 2369  236     <8>   | <7>   234    234   | 234    <5>     <1>   |
| 235   <4>     135   | 258   <6>    <9>   | 238    28      <7>   |
'---------------------'--------------------'----------------------'
```



![](../pic/P2.png)

```
Format（格式）:
"0+"

Output（输出结果）:
000090+40+6+90340+600556+4207900802+6+5+90400750+4+869+2+64+9+7005080+5180406920+60+7130+4+400060000

---

Format（格式）:
"@"

Output（输出结果）:
.-------+-------+-------.
| . . . | . 9 . | . . . |
| . . 3 | 4 . . | . . 5 |
| 5 6 . | 2 . 7 | 9 . . |
:-------+-------+-------:
| 8 . 2 | . . . | . 4 . |
| . 7 5 | . . . | 6 9 . |
| . 4 . | . . . | 5 . 8 |
:-------+-------+-------:
| . . 1 | 8 . 4 | . 6 9 |
| 2 . . | . . 1 | 3 . . |
| . . . | . 6 . | . . . |
'-------+-------+-------'

---

Format（格式）:
"@*:"

Output（输出结果）:
+---------------+---------------+------------------+
| 17   128  78  | 135  <9>  35  | *4*   12378  *6* |
| *9*  128  <3> | <4>  18   *6* | 1278  1278   <5> |
| <5>  <6>  *4* | <2>  138  <7> | <9>   138    13  |
+---------------+---------------+------------------+
| <8>  13   <2> | *6*  *5*  *9* | 17    <4>    137 |
| 13   <7>  <5> | 13   *4*  *8* | <6>   <9>    *2* |
| *6*  <4>  *9* | *7*  123  23  | <5>   13     <8> |
+---------------+---------------+------------------+
| 37   *5*  <1> | <8>  23   <4> | 27    <6>    <9> |
| <2>  89   *6* | 59   *7*  <1> | <3>   58     *4* |
| *4*  389  78  | 359  <6>  235 | 1278  12578  17  |
+---------------+---------------+------------------+
```



![](../pic/P3.png)

```
Format（格式）:
"0+:"

Output（输出结果）:
320009+100+40000+150006130040004903+6+801+60+3812+90+48+10090360004008210+106000+70000010+3+649:515 615 724 825 228 229 731 235 738 748 563 972 574 882 484 584 792 295

---

Format（格式）:
"@:!"

Output（输出结果）:
.----------------.----------------.-----------------.
| <3>  <2>  578  | 4567  478  <9> | <1>  78    678  |
| <4>  789  78   | 26    267  <1> | <5>  3789  3678 |
| 59   <6>  <1>  | <3>   578  57  | <4>  289   278  |
:----------------+----------------+-----------------:
| 257  <4>  <9>  | 57    <3>  <6> | <8>  25    <1>  |
| <6>  57   <3>  | <8>   <1>  <2> | <9>  57    <4>  |
| <8>  <1>  27   | 457   <9>  457 | <3>  <6>   257  |
:----------------+----------------+-----------------:
| 579  357  <4>  | 679   567  <8> | <2>  <1>   35   |
| <1>  359  <6>  | 29    245  45  | <7>  358   358  |
| 257  58   2578 | <1>   57   <3> | <6>  <4>   <9>  |
'----------------'----------------'-----------------'
```
