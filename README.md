# TrayTextPaste
 
Случалось ли вам заполнять длинные формы с большим количеством полей на веб-сайтах? Думаю, каждый сталкивался с ними на Госуслугах и иных подобных порталах. И если разовое заполнение обычно не вызывает затруднений, то внесение множества данных в одну и ту же форму несколько раз (например, на оформление пропусков по списку сотрудников) может отнять существенное количество времени.
Комбинации клавиш для копирования-вставки из текстового/табличного файла, конечно, выручают, но спустя некоторое время и они начинают утомлять.
Для себя я написал небольшую программу для автоматизации заполнения форм, обладающую следующими функциями:
1.	Вставка в поля типовых строк, повторяющихся от формы к форме (например, адреса, различные номера и т.д.).
2.	Реализация последовательности комбинаций клавиш Ctrl+C, Alt+Tab, Ctrl-V в виде одной глобальной «горячей» клавиши.

Первая функция реализована максимально просто и удобно – по нажатию иконки программы в трее можно кликнуть нужный текст для вставки. После этого программа вставит выбранную строку в текущее активное окно, например такое как браузер или Word, Excel. Для ее работы текст нужно заранее ввести в программу, кликнув по иконке в трее правой кнопкой мыши и выбрав пункт «Редактировать строки» (или просто кликнув двойным щелчком по той же иконке). Можно сделать подпункты, если перед дочерними строками добавить, как минимум, два пробела (сами пробелы при вставке текста программой игнорируются).

Вторая функция работает следующим образом. При нажатии сопоставленной в настройках моей программы комбинации клавиш будет произведена эмуляция нажатия последовательно Ctrl+C, Alt+Tab, Ctrl-V. То есть, из текущего активного окна приложения, с которым вы работаете, выделенный текст копируется, делается переключение на предыдущее окно и текст вставляется в поле, которое было активировано заранее. У меня на клавиатуре имеются дополнительные клавиши F13-F19, чем я и воспользовался, привязав к одной из них эту функцию. А при наличии мыши с дополнительными программируемыми кнопками, можно одну из них настроить на выдачу выбранного в моей программе сочетания комбинации клавиш (обычно это проще, чем написать соответствующий самодостаточный макрос для самой мыши).

Полезность второй функции возможно и спорная, но лично мне она очень пригодилась при заполнении большого количества форм из данных файла Excel.

Исходный код этой крошечной программы я решил отдать в общественное достояние на Github. Возможно, кому-нибудь захочется «подогнать» программу «под себя» или посмотреть алгоритмы ее работы.

Для запуска программы требуется компонент .NET Framework 4.8+ (в Win10/11 стоит по умолчанию, в Win7/8 устанавливается автоматически вместе с обновлениями).

