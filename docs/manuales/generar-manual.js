const fs = require("fs");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, LevelFormat,
  HeadingLevel, BorderStyle, WidthType, ShadingType,
  PageNumber, PageBreak, TableOfContents
} = require("docx");

// ── helpers ──────────────────────────────────────────────────────────
const border = { style: BorderStyle.SINGLE, size: 1, color: "BBBBBB" };
const borders = { top: border, bottom: border, left: border, right: border };
const cellMargins = { top: 60, bottom: 60, left: 100, right: 100 };

function cell(text, opts = {}) {
  const { bold, shading, width, align, colSpan } = opts;
  const runs = [];
  if (bold) {
    runs.push(new TextRun({ text, bold: true, font: "Arial", size: 22 }));
  } else {
    runs.push(new TextRun({ text, font: "Arial", size: 22 }));
  }
  const p = align
    ? new Paragraph({ alignment: align, children: runs })
    : new Paragraph({ children: runs });
  return new TableCell({
    borders,
    margins: cellMargins,
    width: width ? { size: width, type: WidthType.DXA } : undefined,
    shading: shading ? { fill: shading, type: ShadingType.CLEAR } : undefined,
    children: [p],
    columnSpan: colSpan,
  });
}

function headerRow(texts, widths) {
  return new TableRow({
    tableHeader: true,
    children: texts.map((t, i) =>
      cell(t, { bold: true, shading: "D5E8F0", width: widths ? widths[i] : undefined })
    ),
  });
}

function dataRow(texts, widths) {
  return new TableRow({
    children: texts.map((t, i) => cell(t, { width: widths ? widths[i] : undefined })),
  });
}

// ── numbering config ─────────────────────────────────────────────────
const numbering = {
  config: [
    {
      reference: "bullets",
      levels: [
        {
          level: 0,
          format: LevelFormat.BULLET,
          text: "\u2022",
          alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 720, hanging: 360 } } },
        },
      ],
    },
    {
      reference: "bullets2",
      levels: [
        {
          level: 0,
          format: LevelFormat.BULLET,
          text: "\u25CB",
          alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 1440, hanging: 360 } } },
        },
      ],
    },
  ],
};

// ── content builders ─────────────────────────────────────────────────
function p(text, opts = {}) {
  const { bold, spacing, align, heading, numbered, italic, size } = opts;
  const runs = [];
  const runOpts = { font: "Arial" };
  if (bold) runOpts.bold = true;
  if (italic) runOpts.italic = true;
  runOpts.size = size || 24;
  runs.push(new TextRun({ text, ...runOpts }));
  const pOpts = { children: runs };
  if (heading) pOpts.heading = heading;
  if (align) pOpts.alignment = align;
  if (spacing) pOpts.spacing = spacing;
  if (numbered) pOpts.numbering = numbered;
  return new Paragraph(pOpts);
}

function bullet(text, level = 0) {
  return new Paragraph({
    numbering: { reference: "bullets", level },
    spacing: { before: 40, after: 40 },
    children: [new TextRun({ text, font: "Arial", size: 24 })],
  });
}

function bullet2(text) {
  return new Paragraph({
    numbering: { reference: "bullets2", level: 0 },
    spacing: { before: 40, after: 40 },
    children: [new TextRun({ text, font: "Arial", size: 24 })],
  });
}

function sectionBreak() {
  return new Paragraph({ children: [new PageBreak()] });
}

// ── build document ───────────────────────────────────────────────────
const doc = new Document({
  numbering,
  styles: {
    default: { document: { run: { font: "Arial", size: 24 } } },
    paragraphStyles: [
      {
        id: "Heading1",
        name: "Heading 1",
        basedOn: "Normal",
        next: "Normal",
        quickFormat: true,
        run: { size: 32, bold: true, font: "Arial", color: "052741" },
        paragraph: { spacing: { before: 360, after: 200 }, outlineLevel: 0 },
      },
      {
        id: "Heading2",
        name: "Heading 2",
        basedOn: "Normal",
        next: "Normal",
        quickFormat: true,
        run: { size: 28, bold: true, font: "Arial", color: "1F4E79" },
        paragraph: { spacing: { before: 280, after: 160 }, outlineLevel: 1 },
      },
      {
        id: "Heading3",
        name: "Heading 3",
        basedOn: "Normal",
        next: "Normal",
        quickFormat: true,
        run: { size: 26, bold: true, font: "Arial", color: "F25C05" },
        paragraph: { spacing: { before: 200, after: 120 }, outlineLevel: 2 },
      },
    ],
  },
  sections: [
    // ═══════════ PORTADA ═══════════
    {
      properties: {
        page: {
          size: { width: 11906, height: 16838 }, // A4
          margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 },
        },
      },
      children: [
        new Paragraph({ spacing: { before: 4000 }, children: [] }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 200 },
          children: [
            new TextRun({
              text: "SISIE",
              font: "Arial",
              size: 56,
              bold: true,
              color: "F25C05",
            }),
          ],
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 100 },
          children: [
            new TextRun({
              text: "Sistema de Gesti\u00F3n de Ventas e Inventario",
              font: "Arial",
              size: 32,
              color: "052741",
            }),
          ],
        }),
        new Paragraph({ spacing: { before: 600 }, children: [] }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { before: 400, after: 200 },
          border: {
            top: { style: BorderStyle.SINGLE, size: 6, color: "F25C05", space: 1 },
            bottom: { style: BorderStyle.SINGLE, size: 6, color: "F25C05", space: 1 },
          },
          children: [
            new TextRun({
              text: "Manual de Usuarios del Sistema",
              font: "Arial",
              size: 40,
              bold: true,
              color: "052741",
            }),
          ],
        }),
        new Paragraph({ spacing: { before: 200 }, children: [] }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { before: 200 },
          children: [
            new TextRun({
              text: "Anexo A",
              font: "Arial",
              size: 28,
              bold: true,
              color: "1F4E79",
            }),
          ],
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 200 },
          children: [
            new TextRun({
              text: "Versi\u00F3n 1.0",
              font: "Arial",
              size: 24,
              color: "555555",
            }),
          ],
        }),
        new Paragraph({ spacing: { before: 3000 }, children: [] }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [
            new TextRun({
              text: "2026",
              font: "Arial",
              size: 24,
              color: "555555",
            }),
          ],
        }),
      ],
    },

    // ═══════════ ÍNDICE / TOC ═══════════
    {
      properties: {
        page: {
          size: { width: 11906, height: 16838 },
          margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 },
        },
      },
      headers: {
        default: new Header({
          children: [
            new Paragraph({
              alignment: AlignmentType.RIGHT,
              border: { bottom: { style: BorderStyle.SINGLE, size: 4, color: "F25C05", space: 4 } },
              children: [new TextRun({ text: "SISIE - Manual de Usuarios", font: "Arial", size: 18, color: "555555", italics: true })],
            }),
          ],
        }),
      },
      footers: {
        default: new Footer({
          children: [
            new Paragraph({
              alignment: AlignmentType.CENTER,
              children: [
                new TextRun({ text: "P\u00E1gina ", font: "Arial", size: 18, color: "888888" }),
                new TextRun({ children: [PageNumber.CURRENT], font: "Arial", size: 18, color: "888888" }),
              ],
            }),
          ],
        }),
      },
      children: [
        p("\u00CDndice", { heading: HeadingLevel.HEADING_1 }),
        new TableOfContents("\\o \"1-3\"", { hyperlink: true, headingStyleRange: "1-3" }),
      ],
    },

    // ═══════════ CONTENIDO ═══════════
    {
      properties: {
        page: {
          size: { width: 11906, height: 16838 },
          margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 },
        },
      },
      headers: {
        default: new Header({
          children: [
            new Paragraph({
              alignment: AlignmentType.RIGHT,
              border: { bottom: { style: BorderStyle.SINGLE, size: 4, color: "F25C05", space: 4 } },
              children: [new TextRun({ text: "SISIE - Manual de Usuarios", font: "Arial", size: 18, color: "555555", italics: true })],
            }),
          ],
        }),
      },
      footers: {
        default: new Footer({
          children: [
            new Paragraph({
              alignment: AlignmentType.CENTER,
              children: [
                new TextRun({ text: "P\u00E1gina ", font: "Arial", size: 18, color: "888888" }),
                new TextRun({ children: [PageNumber.CURRENT], font: "Arial", size: 18, color: "888888" }),
              ],
            }),
          ],
        }),
      },
      children: [
        // ── 1. INTRODUCCIÓN ──
        p("1. Introducci\u00F3n", { heading: HeadingLevel.HEADING_1 }),
        p(
          "El prop\u00F3sito de este documento es describir la utilizaci\u00F3n por parte de los usuarios del sistema SISIE (Sistema de Gesti\u00F3n de Ventas e Inventario), una aplicaci\u00F3n web dise\u00F1ada para la administraci\u00F3n de productos, categor\u00EDas, clientes y ventas en comercios del rubro ferreter\u00EDa y corral\u00F3n.",
          { spacing: { after: 120 } }
        ),

        // ── 2. OBJETIVO ──
        p("2. Objetivo de este manual", { heading: HeadingLevel.HEADING_1 }),
        p(
          "El objetivo es indicar los pasos y procedimientos a realizar para llevar a cabo las distintas tareas y funcionalidades que provee el sistema, permitiendo a los usuarios aprovechar al maximum todas las capacidades de SISIE.",
          { spacing: { after: 120 } }
        ),

        // ── 3. DIRIGIDO A ──
        p("3. Dirigido a", { heading: HeadingLevel.HEADING_1 }),
        p(
          "Para la utilizaci\u00F3n del presente sistema se pueden reconocer dos tipos distintos de perfiles:",
          { spacing: { after: 80 } }
        ),
        bullet(
          "Usuario registrado: es aquel que accede al sistema mediante credenciales (usuario y contrase\u00F1a) con el fin de llevar a cabo tareas administrativas como la gesti\u00F3n de productos, categor\u00EDas, clientes y el registro de ventas."
        ),
        bullet(
          "Usuario administrador: es aquel que tiene control total sobre el sistema, incluyendo la administraci\u00F3n de usuarios y la configuraci\u00F3n general del sistema."
        ),
        p("", { spacing: { after: 80 } }),

        // ── 4. LO QUE DEBEN CONOCER ──
        p("4. Lo que deben conocer", { heading: HeadingLevel.HEADING_1 }),
        p(
          "Los conocimientos m\u00EDnimos que deben tener las personas que operar\u00E1n el sistema y deber\u00E1n utilizar este manual son:",
          { spacing: { after: 80 } }
        ),
        bullet(
          "Usuarios registrados: deben conocer el manejo b\u00E1sico de un navegador web (Google Chrome, Mozilla Firefox, Microsoft Edge) y tener familiaridad con formularios, tablas y men\u00FAs de navegaci\u00F3n web."
        ),
        bullet(
          "Administrador: debe tener conocimientos profundos de la estructura y funcionamiento de la organizaci\u00F3n. Adem\u00E1s es deseable que conozca aspectos fundamentales de administraci\u00F3n de sistemas, redes y configuraci\u00F3n de servidores."
        ),
        p("", { spacing: { after: 80 } }),

        // ── 5. ESPECIFICACIONES TÉCNICAS ──
        p("5. Especificaciones t\u00E9cnicas", { heading: HeadingLevel.HEADING_1 }),
        p(
          "Para la implementaci\u00F3n del sistema se deber\u00E1 contar con estos requerimientos:",
          { spacing: { after: 80 } }
        ),
        p("5.1 Hardware", { heading: HeadingLevel.HEADING_2 }),
        new Table({
          width: { size: 9026, type: WidthType.DXA },
          columnWidths: [4513, 4513],
          rows: [
            headerRow(["Componente", "Especificaci\u00F3n M\u00EDnima"], [4513, 4513]),
            dataRow([
              "Servidor",
              "Disco duro: 500 GB, RAM: 8 GB, CPU: 2 GHz, teclado, mouse y monitor",
            ], [4513, 4513]),
            dataRow([
              "Terminal de usuario",
              "RAM: 4 GB, CPU: 1 GHz, Disco: 40 GB, teclado, mouse, monitor y conexi\u00F3n a red",
            ], [4513, 4513]),
            dataRow([
              "Hardware de red",
              "Switches, routers y cableado (UTP como m\u00EDnimo) que soporten el direccionamiento IP entre los clientes y el servidor",
            ], [4513, 4513]),
            dataRow([
              "UPS (recomendado)",
              "Sistema de alimentaci\u00F3n ininterrumpida para el servidor",
            ], [4513, 4513]),
          ],
        }),
        p("", { spacing: { after: 120 } }),
        p("5.2 Software", { heading: HeadingLevel.HEADING_2 }),
        new Table({
          width: { size: 9026, type: WidthType.DXA },
          columnWidths: [4513, 4513],
          rows: [
            headerRow(["Entorno", "Requerimiento"], [4513, 4513]),
            dataRow(["Servidor", ".NET 8 Runtime o superior (Windows / Linux)"], [4513, 4513]),
            dataRow(["Cliente (navegador)", "Google Chrome 90+, Mozilla Firefox 88+, Microsoft Edge 90+, o cualquier navegador moderno"], [4513, 4513]),
            dataRow(["Base de datos", "SQLite (incluida en la aplicaci\u00F3n, no requiere instalaci\u00F3n separada)"], [4513, 4513]),
            dataRow(["Sistema operativo servidor", "Windows Server 2016+ o Linux (Ubuntu 20.04+, Debian 11+, CentOS 8+)"], [4513, 4513]),
            dataRow(["Sistema operativo cliente", "Windows 7/8/10/11, Linux o macOS"], [4513, 4513]),
          ],
        }),
        p("", { spacing: { after: 120 } }),

        // ── 6. CARACTERÍSTICAS DEL PRODUCTO ──
        p("6. Caracter\u00EDsticas del producto", { heading: HeadingLevel.HEADING_1 }),
        p(
          "El sistema fue desarrollado en C# con .NET 8, utilizando una base de datos SQLite. El mismo fue pensado de manera tal que pueda ser expandido a otros tipos de comercios que requieran gesti\u00F3n de ventas e inventario (gen\u00E9rico).",
          { spacing: { after: 80 } }
        ),
        p(
          "Adem\u00E1s posee una interfaz gr\u00E1fica web amigable, f\u00E1cil de usar, dise\u00F1ada para adaptarse lo m\u00E1s posible a usuarios con distintas capacidades."
        ),
        p("", { spacing: { after: 80 } }),
        p(
          "Tambi\u00E9n posee un alto grado de portabilidad, ya que los clientes no necesitan llevar a cabo una instalaci\u00F3n costosa: solamente se debe acceder al sistema mediante un navegador web desde cualquier computadora de la red."
        ),
        p("", { spacing: { after: 80 } }),
        p(
          "Posee un alto grado de seguridad a la hora de administrar los datos del comercio, gestionando las funcionalidades de cada tipo de usuario mediante autenticaci\u00F3n JWT (JSON Web Tokens) y almacenando registros de todas las acciones llevadas a cabo mediante el mismo."
        ),
        p("", { spacing: { after: 80 } }),

        // ── 7. USO DEL SISTEMA ──
        p("7. Uso del sistema", { heading: HeadingLevel.HEADING_1 }),

        // 7.1 Ingreso
        p("7.1 Ingreso al sistema", { heading: HeadingLevel.HEADING_2 }),
        p(
          "Primeramente se debe abrir un navegador web y acceder a la URL donde se encuentra alojado el sistema SISIE (por ejemplo: http://localhost:5000 o la IP del servidor configurada en la red)."
        ),
        p("", { spacing: { after: 80 } }),
        p(
          "El sistema mostrar\u00E1 la pantalla de inicio de sesi\u00F3n. Aqu\u00ED el usuario deber\u00E1 ingresar su correo electr\u00F3nico y contrase\u00F1a para acceder al sistema."
        ),
        p("", { spacing: { after: 80 } }),
        p(
          "IMAGEN: Pantalla de inicio de sesi\u00F3n con campos de correo y contrase\u00F1a, y el bot\u00F3n \"Entrar\".",
          { italic: true, size: 22, spacing: { after: 80 } }
        ),
        p("Procedimiento:", { bold: true, spacing: { after: 40 } }),
        bullet("Ingresar el correo electr\u00F3nico en el campo correspondiente."),
        bullet("Ingresar la contrase\u00F1a en el campo correspondiente."),
        bullet("Hacer clic en el bot\u00F3n \"Entrar\"."),
        bullet(
          "Si las credenciales son v\u00E1lidas, el sistema redirige al panel principal (Dashboard)."
        ),
        bullet(
          "Si las credenciales son incorrectas, se mostrar\u00E1 un mensaje de error en la parte inferior del formulario."
        ),
        p("", { spacing: { after: 80 } }),
        p(
          "En caso de no poseer una cuenta, se debe hacer clic en el enlace \"Reg\u00EDstrate\" que redirige al formulario de registro.",
          { spacing: { after: 80 } }
        ),
        p("Formulario de registro:", { bold: true, spacing: { after: 40 } }),
        bullet("Completar los campos: Nombre, Correo, Contrase\u00F1a y Confirmar contrase\u00F1a."),
        bullet(
          "Aceptar los t\u00E9rminos y condiciones marcando la casilla correspondiente."
        ),
        bullet("Hacer clic en \"Crear Cuenta\"."),
        bullet(
          "Si el registro es exitoso, el sistema inicia sesi\u00F3n autom\u00E1ticamente y redirige al panel principal."
        ),
        p("", { spacing: { after: 80 } }),
        p(
          "IMAGEN: Pantalla de registro de nuevo usuario con todos los campos.",
          { italic: true, size: 22, spacing: { after: 80 } }
        ),

        // 7.2 Menú principal
        p("7.2 Men\u00FA principal del sistema", { heading: HeadingLevel.HEADING_2 }),
        p(
          "IMAGEN: Panel principal (Dashboard) con las tarjetas de navegaci\u00F3n y el resumen del sistema.",
          { italic: true, size: 22, spacing: { after: 80 } }
        ),
        p(
          "En esta pantalla se muestra el nombre del usuario ingresado (en la parte superior: \"Bienvenido, {nombre}\") y las distintas funciones disponibles en forma de tarjetas de acceso r\u00E1pido:"
        ),
        p("", { spacing: { after: 80 } }),
        new Table({
          width: { size: 9026, type: WidthType.DXA },
          columnWidths: [2000, 3513, 3513],
          rows: [
            headerRow(["Icono", "Opci\u00F3n", "Descripci\u00F3n"], [2000, 3513, 3513]),
            dataRow(
              ["\uD83D\uDCE6", "Productos", "Acceso a la gesti\u00F3n completa del inventario"],
              [2000, 3513, 3513]
            ),
            dataRow(
              ["\uD83C\uDFF7", "Categor\u00EDas", "Administraci\u00F3n de categor\u00EDas de productos"],
              [2000, 3513, 3513]
            ),
            dataRow(
              ["\uD83D\uDCCB", "Nueva Venta", "Registro de ventas y emisi\u00F3n de comprobantes"],
              [2000, 3513, 3513]
            ),
          ],
        }),
        p("", { spacing: { after: 80 } }),
        p(
          "Adem\u00E1s se muestra un resumen del sistema con la cantidad total de productos y los productos con stock bajo.",
          { spacing: { after: 80 } }
        ),
        p(
          "En la parte inferior se encuentra el bot\u00F3n \"Cerrar Sesi\u00F3n\" para salir del sistema.",
          { spacing: { after: 80 } }
        ),

        // 7.3 Productos
        p("7.3 Gesti\u00F3n de Productos", { heading: HeadingLevel.HEADING_2 }),
        p(
          "IMAGEN: Pantalla de gesti\u00F3n de productos con formulario de carga y tabla de listado.",
          { italic: true, size: 22, spacing: { after: 80 } }
        ),
        p(
          "Desde esta pantalla el usuario puede realizar las siguientes operaciones con los productos del inventario:",
          { spacing: { after: 80 } }
        ),

        p("7.3.1 Alta de un nuevo producto", { heading: HeadingLevel.HEADING_3 }),
        bullet("Completar los campos del formulario: Nombre (obligatorio), Categor\u00EDa (obligatorio), Precio (obligatorio), Stock (obligatorio) y Descripci\u00F3n (opcional)."),
        bullet("Hacer clic en el bot\u00F3n \"Guardar Producto\"."),
        bullet(
          "Si los datos son correctos, el producto se registra y aparece en la tabla de listado."
        ),
        bullet(
          "Si hay errores de validaci\u00F3n, se muestra un mensaje indicando el problema."
        ),
        p("", { spacing: { after: 80 } }),

        p("7.3.2 Edici\u00F3n de un producto", { heading: HeadingLevel.HEADING_3 }),
        bullet("Hacer clic en el icono de edici\u00F3n (l\u00E1piz) en la fila del producto deseado."),
        bullet(
          "El formulario se completa autom\u00E1ticamente con los datos del producto."
        ),
        bullet(
          "Modificar los campos necesarios y hacer clic en \"Guardar Producto\"."
        ),
        p("", { spacing: { after: 80 } }),

        p("7.3.3 Activar/Desactivar un producto", { heading: HeadingLevel.HEADING_3 }),
        bullet(
          "Utilizar el interruptor (toggle) en la columna \"Estado\" para activar o desactivar un producto."
        ),
        bullet(
          "Un producto desactivado no aparecer\u00E1 disponible para la venta, pero su registro se conserva en el sistema."
        ),
        p("", { spacing: { after: 80 } }),

        p("7.3.4 B\u00FAsqueda de productos", { heading: HeadingLevel.HEADING_3 }),
        bullet(
          "Utilizar el campo de b\u00FAsqueda en la parte superior de la tabla para filtrar productos por nombre o categor\u00EDa."
        ),
        bullet(
          "La tabla se actualiza en tiempo real mostrando los resultados coincidentes."
        ),
        p("", { spacing: { after: 80 } }),

        // 7.4 Categorías
        p("7.4 Gesti\u00F3n de Categor\u00EDas", { heading: HeadingLevel.HEADING_2 }),
        p(
          "IMAGEN: Pantalla de gesti\u00F3n de categor\u00EDas con formulario y tarjetas de listado.",
          { italic: true, size: 22, spacing: { after: 80 } }
        ),
        p(
          "Desde esta pantalla el usuario puede administrar las categor\u00EDas utilizadas para clasificar los productos:",
          { spacing: { after: 80 } }
        ),

        p("7.4.1 Alta de una nueva categor\u00EDa", { heading: HeadingLevel.HEADING_3 }),
        bullet("Ingresar el nombre de la categor\u00EDa en el campo correspondiente."),
        bullet("Hacer clic en \"Guardar\"."),
        bullet(
          "La nueva categor\u00EDa aparece en el grid de categor\u00EDas con su cantidad de productos asociados."
        ),
        p("", { spacing: { after: 80 } }),

        p("7.4.2 Edici\u00F3n de una categor\u00EDa", { heading: HeadingLevel.HEADING_3 }),
        bullet(
          "Hacer clic en el icono de edici\u00F3n (l\u00E1piz) en la tarjeta de la categor\u00EDa."
        ),
        bullet("Modificar el nombre y hacer clic en \"Actualizar\"."),
        p("", { spacing: { after: 80 } }),

        p("7.4.3 Eliminaci\u00F3n de una categor\u00EDa", { heading: HeadingLevel.HEADING_3 }),
        bullet(
          "Hacer clic en el icono de eliminar (papelera) en la tarjeta de la categor\u00EDa."
        ),
        bullet(
          "Solo se pueden eliminar categor\u00EDas que no tengan productos asociados."
        ),
        bullet(
          "El sistema solicitar\u00E1 confirmaci\u00F3n antes de eliminar."
        ),
        p("", { spacing: { after: 80 } }),

        // 7.5 Ventas
        p("7.5 Registro de Ventas", { heading: HeadingLevel.HEADING_2 }),
        p(
          "IMAGEN: Pantalla de nueva venta con datos del cliente, selector de productos, m\u00E9todo de pago y tipo de entrega.",
          { italic: true, size: 22, spacing: { after: 80 } }
        ),
        p(
          "Esta es la pantalla principal de operaci\u00F3n del sistema. Permite registrar una venta completa:",
          { spacing: { after: 80 } }
        ),

        p("7.5.1 Datos del Cliente", { heading: HeadingLevel.HEADING_3 }),
        bullet(
          "Buscar un cliente existente utilizando el buscador (por DNI o nombre)."
        ),
        bullet(
          "Si el cliente no existe, completar manualmente los campos: Cliente, DNI/CUIT, Tel\u00E9fono y Email."
        ),
        p("", { spacing: { after: 80 } }),

        p("7.5.2 Agregar productos a la venta", { heading: HeadingLevel.HEADING_3 }),
        bullet(
          "Utilizar el buscador de productos para encontrar el producto deseado."
        ),
        bullet("Seleccionar el producto de la lista de resultados."),
        bullet("Indicar la cantidad y hacer clic en \"Agregar\"."),
        bullet(
          "El producto aparece en la tabla de productos de la venta con su precio, cantidad y subtotal."
        ),
        bullet(
          "Repetir el proceso para agregar todos los productos deseados."
        ),
        bullet(
          "Se puede quitar un producto haciendo clic en el icono de eliminar (X)."
        ),
        p("", { spacing: { after: 80 } }),

        p("7.5.3 M\u00E9todo de pago y entrega", { heading: HeadingLevel.HEADING_3 }),
        bullet(
          "Seleccionar el m\u00E9todo de pago: Efectivo, Tarjeta D\u00E9bito, Tarjeta Cr\u00E9dito o Transferencia."
        ),
        bullet(
          "Seleccionar el tipo de entrega: Venta por Mostrador, Retiro en Sucursal o Env\u00EDo a Domicilio."
        ),
        bullet(
          "Si se selecciona \"Env\u00EDo a Domicilio\", completar los campos de direcci\u00F3n (provincia, ciudad, direcci\u00F3n, departamento y c\u00F3digo postal)."
        ),
        p("", { spacing: { after: 80 } }),

        p("7.5.4 Descuento y confirmaci\u00F3n", { heading: HeadingLevel.HEADING_3 }),
        bullet(
          "Opcionalmente ingresar un porcentaje de descuento en el campo correspondiente del panel de resumen."
        ),
        bullet(
          "Verificar el resumen de la venta (productos, subtotal, descuento y total)."
        ),
        bullet("Hacer clic en \"Confirmar Venta\" para finalizar."),
        bullet(
          "El sistema procesa la venta, descuenta el stock de los productos y genera un comprobante."
        ),
        bullet("La venta se registra en el historial de ventas.",
        p("", { spacing: { after: 80 } })),

        p("7.5.5 Historial de Ventas", { heading: HeadingLevel.HEADING_3 }),
        p(
          "En la parte inferior de la pantalla de ventas se muestra el historial de todas las ventas registradas, con los siguientes datos:"
        ),
        bullet("N\u00FAmero de venta"),
        bullet("Fecha y hora"),
        bullet("Nombre del cliente"),
        bullet("Cantidad de productos"),
        bullet("Total de la venta"),
        bullet("Tipo de entrega"),
        bullet("Estado (Completado, Pendiente, Anulado)"),
        p("", { spacing: { after: 80 } }),

        // 7.6 Cierre
        p("7.6 Cierre de sesi\u00F3n", { heading: HeadingLevel.HEADING_2 }),
        p(
          "Para cerrar la sesi\u00F3n, hacer clic en el bot\u00F3n \"Cerrar Sesi\u00F3n\" (icono de salida) ubicado en la parte inferior del panel principal o en la barra de navegaci\u00F3n de las pantallas secundarias."
        ),
        p("", { spacing: { after: 80 } }),
        p(
          "IMAGEN: Bot\u00F3n de cierre de sesi\u00F3n en la barra de navegaci\u00F3n.",
          { italic: true, size: 22, spacing: { after: 80 } }
        ),
        p(
          "El sistema eliminar\u00E1 el token de autenticaci\u00F3n almacenado y redirigir\u00E1 a la pantalla de inicio de sesi\u00F3n."
        ),
        p("", { spacing: { after: 80 } }),

        // ── 8. Atajos de teclado ──
        p("8. Atajos de teclado", { heading: HeadingLevel.HEADING_1 }),
        p(
          "SISIE posee atajos de teclado para agilizar la navegaci\u00F3n. Estos atajos funcionan presionando la tecla Ctrl junto con otra tecla:"
        ),
        p("", { spacing: { after: 80 } }),
        new Table({
          width: { size: 9026, type: WidthType.DXA },
          columnWidths: [2000, 3513, 3513],
          rows: [
            headerRow(["Atajo", "Funci\u00F3n", "Pantalla de destino"], [2000, 3513, 3513]),
            dataRow(["Ctrl + P", "Ir a Productos", "Gesti\u00F3n de Productos"], [2000, 3513, 3513]),
            dataRow(["Ctrl + K", "Ir a Categor\u00EDas", "Gesti\u00F3n de Categor\u00EDas"], [2000, 3513, 3513]),
            dataRow(["Ctrl + V", "Ir a Ventas", "Nueva Venta"], [2000, 3513, 3513]),
            dataRow(["Ctrl + L", "Cerrar sesi\u00F3n", "Login"], [2000, 3513, 3513]),
            dataRow(["Ctrl + I", "Ir al inicio", "Dashboard"], [2000, 3513, 3513]),
            dataRow(["Ctrl + Enter", "Enviar formulario", "Login / Registro"], [2000, 3513, 3513]),
            dataRow(["Escape", "Volver / Cerrar modal", "Registro / Modales"], [2000, 3513, 3513]),
          ],
        }),
        p("", { spacing: { after: 120 } }),

        // ── 9. Resolución de problemas frecuentes ──
        p("9. Resoluci\u00F3n de problemas frecuentes", { heading: HeadingLevel.HEADING_1 }),

        p("9.1 No puedo iniciar sesi\u00F3n", { heading: HeadingLevel.HEADING_2 }),
        bullet("Verifique que el correo electr\u00F3nico y la contrase\u00F1a sean correctos."),
        bullet("Aseg\u00FArese de que el teclado no tenga bloqueado Bloq May\u00FAs."),
        bullet(
          "Si olvid\u00F3 la contrase\u00F1a, contacte al administrador del sistema."
        ),
        p("", { spacing: { after: 80 } }),

        p("9.2 No encuentro un producto en la venta", { heading: HeadingLevel.HEADING_2 }),
        bullet("Verifique que el producto est\u00E9 activo (toggle en verde en la pantalla de productos)."),
        bullet("Verifique que el producto tenga stock disponible (mayor a 0)."),
        bullet(
          "Utilice el buscador escribiendo al menos parte del nombre del producto."
        ),
        p("", { spacing: { after: 80 } }),

        p("9.3 No puedo eliminar una categor\u00EDa", { heading: HeadingLevel.HEADING_2 }),
        bullet(
          "Las categor\u00EDas con productos asociados no pueden eliminarse. Primero reasigne o elimine los productos de esa categor\u00EDa."
        ),
        p("", { spacing: { after: 120 } }),

        // ── 10. ESPECIFICACIÓN DE CAPTURAS ──
        p("10. Especificaci\u00F3n de capturas de pantalla", { heading: HeadingLevel.HEADING_1 }),
        p(
          "A continuaci\u00F3n se detallan las capturas de pantalla necesarias para complementar este manual, indicando su ubicaci\u00F3n exacta dentro del documento y el contenido que deben mostrar:",
          { spacing: { after: 120 } }
        ),

        p("Captura N\u00B0 1 \u2014 Pantalla de inicio de sesi\u00F3n", { heading: HeadingLevel.HEADING_2 }),
        p("Ubicaci\u00F3n en el manual: Secci\u00F3n 7.1 (Ingreso al sistema), luego del p\u00E1rrafo \"El sistema mostrar\u00E1 la pantalla de inicio de sesi\u00F3n...\".", { spacing: { after: 40 } }),
        p("Qu\u00E9 debe mostrar:", { bold: true, spacing: { after: 40 } }),
        bullet("La pantalla completa de login.html en un navegador."),
        bullet("El logo de SISIE en la parte superior."),
        bullet("El t\u00EDtulo \"Iniciar Sesi\u00F3n\"."),
        bullet("El campo de correo electr\u00F3nico con icono de sobre."),
        bullet("El campo de contrase\u00F1a con icono de candado y bot\u00F3n de mostrar/ocultar."),
        bullet("El bot\u00F3n \"Entrar\" de color naranja."),
        bullet("El enlace \"Reg\u00EDstrate\" en la parte inferior."),
        p("", { spacing: { after: 120 } }),

        p("Captura N\u00B0 2 \u2014 Pantalla de registro de usuario", { heading: HeadingLevel.HEADING_2 }),
        p("Ubicaci\u00F3n en el manual: Secci\u00F3n 7.1 (Ingreso al sistema), luego del p\u00E1rrafo \"En caso de no poseer una cuenta...\".", { spacing: { after: 40 } }),
        p("Qu\u00E9 debe mostrar:", { bold: true, spacing: { after: 40 } }),
        bullet("La pantalla completa de register.html."),
        bullet("El logo de SISIE en la parte superior."),
        bullet("El t\u00EDtulo \"Crear Cuenta\"."),
        bullet("Los campos: Nombre, Correo, Contrase\u00F1a, Confirmar."),
        bullet("El checkbox de t\u00E9rminos y condiciones."),
        bullet("El bot\u00F3n \"Crear Cuenta\"."),
        p("", { spacing: { after: 120 } }),

        p("Captura N\u00B0 3 \u2014 Panel principal (Dashboard)", { heading: HeadingLevel.HEADING_2 }),
        p("Ubicaci\u00F3n en el manual: Secci\u00F3n 7.2 (Men\u00FA principal del sistema), al inicio del punto.", { spacing: { after: 40 } }),
        p("Qu\u00E9 debe mostrar:", { bold: true, spacing: { after: 40 } }),
        bullet("El dashboard completo (index.html)."),
        bullet("El saludo \"Bienvenido, {nombre del usuario}\"."),
        bullet("Las tres tarjetas de acceso: Productos (icono de caja), Categor\u00EDas (icono de etiqueta), Nueva Venta (icono de caja registradora)."),
        bullet("La secci\u00F3n \"Resumen del Sistema\" con Total Productos y Stock Bajo."),
        bullet("El bot\u00F3n \"Cerrar Sesi\u00F3n\" en la parte inferior."),
        p("", { spacing: { after: 120 } }),

        p("Captura N\u00B0 4 \u2014 Gesti\u00F3n de Productos", { heading: HeadingLevel.HEADING_2 }),
        p("Ubicaci\u00F3n en el manual: Secci\u00F3n 7.3 (Gesti\u00F3n de Productos), al inicio del punto.", { spacing: { after: 40 } }),
        p("Qu\u00E9 debe mostrar:", { bold: true, spacing: { after: 40 } }),
        bullet("La pantalla completa de productos.html."),
        bullet("La barra de navegaci\u00F3n superior con el logo SISIE y enlaces."),
        bullet("El formulario de nuevo producto con todos los campos visibles."),
        bullet("La tabla de listado de productos con al menos 3 filas de ejemplo."),
        bullet("Los interruptores de estado (toggle) en la columna Estado."),
        bullet("Los iconos de edici\u00F3n en cada fila."),
        bullet("El campo de b\u00FAsqueda y la paginaci\u00F3n."),
        p("", { spacing: { after: 120 } }),

        p("Captura N\u00B0 5 \u2014 Gesti\u00F3n de Categor\u00EDas", { heading: HeadingLevel.HEADING_2 }),
        p("Ubicaci\u00F3n en el manual: Secci\u00F3n 7.4 (Gesti\u00F3n de Categor\u00EDas), al inicio del punto.", { spacing: { after: 40 } }),
        p("Qu\u00E9 debe mostrar:", { bold: true, spacing: { after: 40 } }),
        bullet("La pantalla completa de categorias.html."),
        bullet("El formulario de nueva categor\u00EDa (campo de texto + bot\u00F3n Guardar)."),
        bullet("El grid de categor\u00EDas con al menos 3 tarjetas visibles."),
        bullet("Cada tarjeta mostrando el nombre de la categor\u00EDa y la cantidad de productos."),
        bullet("Los iconos de edici\u00F3n y eliminaci\u00F3n en cada tarjeta."),
        p("", { spacing: { after: 120 } }),

        p("Captura N\u00B0 6 \u2014 Nueva Venta", { heading: HeadingLevel.HEADING_2 }),
        p("Ubicaci\u00F3n en el manual: Secci\u00F3n 7.5 (Registro de Ventas), al inicio del punto.", { spacing: { after: 40 } }),
        p("Qu\u00E9 debe mostrar:", { bold: true, spacing: { after: 40 } }),
        bullet("La pantalla completa de ventas.html con todos los paneles visibles."),
        bullet("La secci\u00F3n \"Datos del Cliente\" con el buscador y los campos."),
        bullet("La secci\u00F3n \"Productos\" con el buscador, la cantidad y la tabla de productos agregados."),
        bullet("La secci\u00F3n \"M\u00E9todo de Pago\" con las opciones seleccionables."),
        bullet("La secci\u00F3n \"Tipo de Entrega\" con las opciones seleccionables."),
        bullet("El panel de \"Resumen\" a la derecha con el subtotal, descuento y total."),
        bullet("El bot\u00F3n \"Confirmar Venta\"."),
        bullet("La secci\u00F3n \"Historial de Ventas\" en la parte inferior."),
        p("", { spacing: { after: 120 } }),

        p("Captura N\u00B0 7 \u2014 Bot\u00F3n de Cerrar Sesi\u00F3n", { heading: HeadingLevel.HEADING_2 }),
        p("Ubicaci\u00F3n en el manual: Secci\u00F3n 7.6 (Cierre de sesi\u00F3n), luego del p\u00E1rrafo \"Para cerrar la sesi\u00F3n...\".", { spacing: { after: 40 } }),
        p("Qu\u00E9 debe mostrar:", { bold: true, spacing: { after: 40 } }),
        bullet("Un detalle/recorte de la barra de navegaci\u00F3n mostrando el icono de salida (puerta con flecha) en color rojo."),
        bullet("O bien el pie de p\u00E1gina del dashboard con el enlace \"Cerrar Sesi\u00F3n\"."),
        p("", { spacing: { after: 120 } }),

        // ── Nota final ──
        p(
          "--- FIN DEL MANUAL ---",
          {
            align: AlignmentType.CENTER,
            bold: true,
            spacing: { before: 600 },
          }
        ),
      ],
    },
  ],
});

// ── generate ─────────────────────────────────────────────────────────
const outPath = "C:\\Users\\Usuario\\Desktop\\proyecto-SISIE\\docs\\manuales\\Manual-de-Usuarios-SISIE.docx";
Packer.toBuffer(doc).then((buffer) => {
  fs.writeFileSync(outPath, buffer);
  console.log("Documento generado:", outPath);
});
