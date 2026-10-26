"""Generate the fixed-layout class diagrams with Pillow."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

OUT = Path(__file__).resolve().parents[1] / "docs"
INK = "#172B3A"
LINE = "#526879"
BLUE = "#EAF1F7"


def font(size, bold=False):
    candidates = [
        Path("/System/Library/Fonts/Supplemental") / ("Arial Bold.ttf" if bold else "Arial.ttf"),
        Path("/usr/share/fonts/truetype/dejavu") / ("DejaVuSans-Bold.ttf" if bold else "DejaVuSans.ttf"),
        Path("C:/Windows/Fonts") / ("arialbd.ttf" if bold else "arial.ttf"),
    ]
    for candidate in candidates:
        if candidate.exists():
            return ImageFont.truetype(str(candidate), size)
    raise RuntimeError("Install Arial or DejaVu Sans to render the diagrams.")


class Diagram:
    def __init__(self, height, title, subtitle):
        self.image = Image.new("RGB", (1800, height), "white")
        self.d = ImageDraw.Draw(self.image)
        self.text(60, 35, title, 38, True)
        self.text(60, 91, subtitle, 24)

    def text(self, x, y, value, size=25, bold=False):
        width = self.d.textlength(value, font=font(size, bold))
        assert x >= 0 and x + width <= self.image.width, value
        assert y + size + 8 <= self.image.height, value
        self.d.text((x, y), value, font=font(size, bold), fill=INK)

    def center(self, x, y, width, value, size, bold=False):
        tw = self.d.textlength(value, font=font(size, bold))
        assert tw <= width - 20, value
        self.text(x + (width - tw) / 2, y, value, size, bold)

    def box(self, x, y, width, title, fields=(), methods=(), tag=None, size=24):
        header = 85 if tag else 60
        height = header + sum(22 + 35 * len(items) for items in (fields, methods) if items)
        assert x + width <= self.image.width and y + height <= self.image.height
        self.d.rectangle((x, y, x + width, y + height), fill="white", outline=LINE, width=2)
        self.d.rectangle((x + 1, y + 1, x + width - 1, y + header), fill=BLUE)
        if tag:
            self.center(x, y + 10, width, tag, 20)
        self.center(x, y + (39 if tag else 15), width, title, 28, True)
        cy = y + header
        for items in (fields, methods):
            if not items:
                continue
            self.d.line((x, cy, x + width, cy), fill=LINE, width=2)
            for item in items:
                assert self.d.textlength(item, font=font(size)) <= width - 28, item
                self.text(x + 14, cy + 12, item, size)
                cy += 35
            cy += 22
        self.d.rectangle((x, y, x + width, y + height), outline=LINE, width=2)
        return (x, y, width, height)

    def path(self, points, dashed=False, triangle=False, arrow=False):
        for a, b in zip(points, points[1:]):
            dx, dy = b[0] - a[0], b[1] - a[1]
            length = (dx * dx + dy * dy) ** 0.5
            assert length > 0
            if not dashed:
                self.d.line((a, b), fill=LINE, width=3)
            else:
                for n in range(0, int(length), 17):
                    end = min(n + 9, length)
                    self.d.line((a[0] + dx * n / length, a[1] + dy * n / length,
                                 a[0] + dx * end / length, a[1] + dy * end / length),
                                fill=LINE, width=3)
        if triangle or arrow:
            a, b = points[-2:]
            dx, dy = b[0] - a[0], b[1] - a[1]
            length = (dx * dx + dy * dy) ** 0.5
            ux, uy = dx / length, dy / length
            left = (b[0] - ux * 22 - uy * 11, b[1] - uy * 22 + ux * 11)
            right = (b[0] - ux * 22 + uy * 11, b[1] - uy * 22 - ux * 11)
            if triangle:
                self.d.polygon((b, left, right), fill="white", outline=LINE, width=3)
            else:
                self.d.line((left, b, right), fill=LINE, width=3)

    def composition(self, x, y, bottom):
        self.d.polygon(((x, y), (x - 11, y + 17), (x, y + 34), (x + 11, y + 17)),
                       fill=LINE, outline=LINE)
        self.path([(x, y + 34), (x, bottom)])

    def save(self, name):
        OUT.mkdir(exist_ok=True)
        self.image.save(OUT / name)
        return self.image


def domain():
    a = Diagram(1180, "01  Вектор та правила порівняння",
                "Lab2.Domain  •  Дані вектора та два незалежні критерії порівняння")
    comparable = a.box(60, 170, 720, "IComparable<Vector>",
                       methods=["+ CompareTo(other: Vector?): int"], tag="«interface»")
    comparer = a.box(1000, 170, 740, "IComparer<Vector>",
                    methods=["+ Compare(first: Vector?, second: Vector?): int"], tag="«interface»")
    vector = a.box(60, 460, 720, "Vector",
                   ["+ LineColor: string {get}", "+ X: double {get}", "+ Y: double {get}",
                    "+ Length: double {get}"],
                   ["+ Vector(lineColor: string, x: double, y: double)",
                    "+ Extend(distance: double): Vector",
                    "+ CompareTo(other: Vector?): int", "+ ToString(): string"],
                   tag="«sealed»", size=23)
    a.box(1000, 460, 740, "VectorColorComparer",
          ["− ColorOrder: StringComparer {static, readonly}"],
          ["+ Compare(first: Vector?, second: Vector?): int"], tag="«sealed»", size=23)
    a.path([(420, 460), (420, 170 + comparable[3])], dashed=True, triangle=True)
    a.path([(1370, 460), (1370, 170 + comparer[3])], dashed=True, triangle=True)
    a.path([(1000, 570), (780, 570)], dashed=True, arrow=True)
    a.text(800, 529, "використовує", 21)
    a.text(1000, 720, "Основний критерій: колір (uk-UA, без регістру).", 24)
    a.text(1000, 758, "При однаковому кольорі — Vector.CompareTo.", 24)
    a.text(60, 960, "CompareTo: довжина → X → Y → колір.", 25, True)
    a.text(60, 1005, "Extend(2): (3; 4), L = 5 → (4,2; 5,6), L = 7. Повертає новий об’єкт.", 25)
    a.text(60, 1070, "Пунктир + порожній трикутник — реалізація інтерфейсу; відкрита стрілка — залежність.", 23)
    a.text(60, 1110, "+ public    − private    •    Показані основні члени; властивості Vector — лише для читання.", 22)
    return a.save("domain.png")


def tree():
    a = Diagram(1600, "02  Узагальнене дерево та власний ітератор",
                "Lab2.Collections  •  Вкладені Node і PreorderEnumerator належать BinarySearchTree<T>")
    enumerable = a.box(70, 180, 760, "IEnumerable<T>",
                        methods=["+ GetEnumerator(): IEnumerator<T>"], tag="«interface»")
    iterator = a.box(1000, 180, 730, "IEnumerator<T>", ["+ Current: T {get}"],
                     ["+ MoveNext(): bool", "+ Reset(): void", "+ Dispose(): void"], tag="«interface»")
    tree_box = a.box(70, 460, 760, "BinarySearchTree<T>",
                     ["− _root: Node?", "− _comparer: IComparer<T>", "− _version: int",
                      "+ Count: int {get; private set}"],
                     ["+ BinarySearchTree(comparer: IComparer<T>?)",
                      "+ Add(item: T): void", "+ Contains(item: T): bool",
                      "+ ToSortedArray(): T[]", "+ GetEnumerator(): IEnumerator<T>"],
                     tag="«sealed»  where T : class", size=23)
    a.box(1000, 550, 730, "PreorderEnumerator",
          ["− _tree: BinarySearchTree<T>", "− _pending: Stack<Node>",
           "− _current: Node?", "− _version: int", "+ Current: T {get}"],
          ["+ MoveNext(): bool", "+ Reset(): void", "+ Dispose(): void"],
          tag="«nested private sealed»", size=23)
    a.path([(450, 460), (450, 180 + enumerable[3])], dashed=True, triangle=True)
    a.path([(1365, 550), (1365, 180 + iterator[3])], dashed=True, triangle=True)
    a.path([(830, 720), (1000, 720)], dashed=True, arrow=True)
    a.text(845, 680, "створює", 21)
    a.path([(1000, 820), (830, 820)], arrow=True)
    a.text(866, 840, "_tree", 21)
    node = a.box(70, 1160, 760, "Node",
                  ["+ Value: T {get}", "+ Left: Node? {get; set}", "+ Right: Node? {get; set}"],
                  ["+ Node(value: T)"], tag="«nested private sealed»", size=23)
    a.composition(450, 460 + tree_box[3], 1160)
    a.text(470, 1020, "_root: 0..1", 23)
    a.path([(830, 1250), (920, 1250), (920, 1390), (830, 1390)], arrow=True)
    a.text(955, 1300, "Left / Right: 0..1", 24)
    a.text(1000, 1060, "Стек: спочатку Push(Right), потім Push(Left).", 24)
    a.text(1000, 1100, "Pop() дістає Left раніше — обхід preorder.", 24)
    a.text(955, 1350, "Посилання на вузли того самого класу.", 23)
    a.text(60, 1485, "Чорний ромб — дерево створює та містить вузли; стрілки показують використання.", 23)
    a.text(60, 1525, "Також реалізовані неузагальнені IEnumerable та IEnumerator; допоміжні члени опущені.", 22)
    return a.save("tree.png")


def application():
    a = Diagram(1200, "03  Застосунок та використані контейнери",
                "Lab2.App  •  Program запускає меню або демонстрацію; дані залишаються у пам’яті")
    program = a.box(60, 190, 520, "Program", methods=["− Main(args: string[]): int"], tag="«static»")
    menu = a.box(640, 190, 520, "ConsoleMenu", ["− _vectors: Vector[]"],
                  ["+ Run(): void", "− ReadVectors(): void", "− ExtendVector(): void"], tag="«sealed»")
    demo = a.box(1220, 190, 520, "Demonstration",
                  methods=["+ CreateVectors(): Vector[]", "+ RunAll(source: Vector[]): void",
                           "+ ShowCollections(source: Vector[]): void",
                           "+ ShowTree(source: Vector[]): void",
                           "+ Print(title: string, items: IEnumerable): void"],
                  tag="«static»", size=20)
    a.path([(580, 260), (640, 260)], dashed=True, arrow=True)
    a.path([(1160, 260), (1220, 260)], dashed=True, arrow=True)
    a.path([(320, 190), (320, 145), (1480, 145), (1480, 190)], dashed=True, arrow=True)
    a.text(815, 157, "--demo", 23, True)
    a.text(60, 550, "У ShowTree: BinarySearchTree<Vector> та BinarySearchTree<string> (див. схему 02).", 25)
    a.text(60, 592, "У ShowCollections: три контейнери з однаковими початковими векторами.", 25)
    a.path([(1480, 190 + demo[3]), (1480, 645), (900, 645), (900, 680)], dashed=True)
    a.path([(325, 680), (1475, 680)], dashed=True)
    for x in [325, 900, 1475]:
        a.path([(x, 680), (x, 730)], dashed=True, arrow=True)
    a.box(60, 730, 530, "Vector[]", ["+ Length: int {get}"], tag="«масив .NET»")
    a.box(635, 730, 530, "ArrayList", ["+ Count: int {get}"],
          ["+ Add(value: object): int", "+ Remove(value: object): void",
           "+ IndexOf(value: object): int"], tag="«System.Collections»", size=23)
    a.box(1210, 730, 530, "List<Vector>", ["+ Count: int {get}"],
          ["+ Add(item: Vector): void", "+ Remove(item: Vector): bool",
           "+ IndexOf(item: Vector): int"], tag="«System.Collections.Generic»", size=23)
    a.text(60, 920, "Array.Resize створює новий масив.", 23)
    a.text(60, 958, "Це статичний метод System.Array,", 23)
    a.text(60, 996, "а не метод самого Vector[].", 23)
    a.text(635, 1040, "Елементи мають тип object.", 23)
    a.text(1210, 1040, "Елементи мають тип Vector.", 23)
    a.text(60, 1120, "Пунктир зі стрілкою — використання / створення. Для контейнерів показано лише потрібні члени.", 22)
    a.text(60, 1158, "Program також викликає Demonstration напряму з аргументом --demo. + public    − private", 22)
    return a.save("application.png")


if __name__ == "__main__":
    images = [domain(), tree(), application()]
    combined = Image.new("RGB", (1800, sum(i.height for i in images) + 80), "#DCE5EB")
    y = 0
    for image in images:
        combined.paste(image, (0, y))
        y += image.height + 40
    combined.save(OUT / "lab2-diagram.png")
    print(OUT)
