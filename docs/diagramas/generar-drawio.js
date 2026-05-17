const fs = require("fs");

// ── helpers ──────────────────────────────────────────────────────────
let idCounter = 10;
function nextId() { return idCounter++; }

function esc(s) {
  if (!s) return "";
  return s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
}

function childCell(id, value, style, y, w, h, parent) {
  return `<mxCell id="${id}" value="${esc(value)}" style="${style}" vertex="1" parent="${parent}">
  <mxGeometry y="${y}" width="${w}" height="${h}" as="geometry" />
</mxCell>`;
}

function edge(id, style, source, target, label = "") {
  return `<mxCell id="${id}" value="${esc(label)}" style="${style}" edge="1" source="${source}" target="${target}" parent="1">
  <mxGeometry relative="1" as="geometry" />
</mxCell>`;
}

let cells = [];

// ── Class definition ─────────────────────────────────────────────────
function addClase(opts) {
  const {
    name, estereotipo, x, y, w, h,
    atributos = [], metodos = [],
    fillColor = "#dae8fc", strokeColor = "#6c8ebf",
    esInterfaz = false
  } = opts;

  const clsId = `cls_${name.replace(/[^a-zA-Z0-9]/g, "_")}`;
  const fontStyle = esInterfaz ? 3 : 1;
  const headerLabel = esInterfaz ? `\u00ABinterface\u00BB\n${name}` : (estereotipo ? `\u00AB${estereotipo}\u00BB\n${name}` : name);

  cells.push(`<mxCell id="${clsId}" value="${esc(headerLabel)}"
    style="swimlane;fontStyle=${fontStyle};align=center;startSize=40;fillColor=${fillColor};strokeColor=${strokeColor};fontSize=12;"
    vertex="1" parent="1">
  <mxGeometry x="${x}" y="${y}" width="${w}" height="${h}" as="geometry" />
</mxCell>`);

  // Divider after header
  const d1Id = `${clsId}_d1`;
  cells.push(`<mxCell id="${d1Id}" value=""
    style="swimlane;startSize=0;fillColor=none;strokeColor=${strokeColor};"
    vertex="1" parent="${clsId}">
  <mxGeometry y="40" width="${w}" height="1" as="geometry" />
</mxCell>`);

  let curY = 50;
  const rowH = 22;

  atributos.forEach((a, i) => {
    const aId = `${clsId}_a${i}`;
    cells.push(childCell(aId, a, "text;html=1;align=left;verticalAlign=top;spacingLeft=5;whiteSpace=wrap;overflow=hidden;rotatable=0;fontSize=11;", curY, w, rowH, clsId));
    curY += rowH;
  });

  if (metodos.length > 0) {
    const d2Id = `${clsId}_d2`;
    cells.push(`<mxCell id="${d2Id}" value=""
      style="swimlane;startSize=0;fillColor=none;strokeColor=${strokeColor};"
      vertex="1" parent="${clsId}">
    <mxGeometry y="${curY}" width="${w}" height="1" as="geometry" />
  </mxCell>`);
    curY += 1;
  }

  metodos.forEach((m, i) => {
    const mId = `${clsId}_m${i}`;
    cells.push(childCell(mId, m, "text;html=1;align=left;verticalAlign=top;spacingLeft=5;whiteSpace=wrap;overflow=hidden;rotatable=0;fontSize=11;", curY, w, rowH, clsId));
    curY += rowH;
  });

  return clsId;
}

function addRel(sourceId, targetId, style, label = "") {
  const id = `rel_${nextId()}`;
  cells.push(edge(id, style, sourceId, targetId, label));
}

// ═══════════════════════════════════════════════════════════════════════
//  BUILD THE DIAGRAM
// ═══════════════════════════════════════════════════════════════════════

let output = `<?xml version="1.0" encoding="UTF-8"?>
<mxfile host="Electron" version="26.0.0">
  <diagram id="diagrama-clases" name="Diagrama de Clases">
    <mxGraphModel dx="1422" dy="762" grid="1" gridSize="10" guides="1"
                  tooltips="1" connect="1" arrows="1" fold="1"
                  page="1" pageScale="1" pageWidth="2800" pageHeight="2200"
                  math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />

        <mxCell id="2" value="SISIE - Diagrama de Clases UML"
                style="text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;whiteSpace=wrap;rounded=0;fontSize=18;fontStyle=1;fontColor=#052741;"
                vertex="1" parent="1">
          <mxGeometry x="700" y="10" width="1000" height="40" as="geometry" />
        </mxCell>
`;

const azul = { fillColor: "#dae8fc", strokeColor: "#6c8ebf" };
const gris = { fillColor: "#f5f5f5", strokeColor: "#666666", esInterfaz: true };
const amarillo = { fillColor: "#fff2cc", strokeColor: "#d6b656" };
const violeta = { fillColor: "#e1d5e7", strokeColor: "#9673a6", esInterfaz: true };

// ════════ CLASES DE DOMINIO (sin estereotipo, azules) ════════

addClase({ name: "ApplicationUser", x: 50, y: 80, w: 230, h: 150,
  atributos: [
    "+ NombreCompleto: string?",
    "+ FechaCreacion: DateTime",
    "+ Activo: bool"
  ],
  metodos: [
    "+ IdentityUser (hereda)"
  ],
  ...azul
});

addClase({ name: "Usuario", x: 320, y: 80, w: 240, h: 240,
  atributos: [
    "+ Id: int",
    "+ NombreUsuario: string",
    "+ PasswordHash: string",
    "+ FechaCreacion: DateTime",
    "+ Activo: bool",
    "+ IdContacto: int",
    "+ Contacto: Contacto? (navegaci\u00f3n)",
    "+ Ventas: ICollection&lt;Venta&gt; (navegaci\u00f3n)"
  ],
  ...azul
});

addClase({ name: "Contacto", x: 600, y: 80, w: 210, h: 130,
  atributos: [
    "+ Id: int",
    "+ Email: string",
    "+ Telefono: int"
  ],
  ...azul
});

addClase({ name: "Categoria", x: 850, y: 80, w: 220, h: 130,
  atributos: [
    "+ Id: int",
    "+ NombreCategoria: string",
    "+ Productos: ICollection&lt;Producto&gt; (navegaci\u00f3n)"
  ],
  ...azul
});

addClase({ name: "Producto", x: 1110, y: 80, w: 260, h: 270,
  atributos: [
    "+ Id: int",
    "+ NombreProducto: string",
    "+ Descripcion: string?",
    "+ PrecioUnitario: decimal",
    "+ Stock: int",
    "+ FechaCreacion: DateTime",
    "+ Activo: bool",
    "+ IdCategoria: int",
    "+ Categoria: Categoria? (navegaci\u00f3n)",
    "+ Detalles: ICollection&lt;DetalleVenta&gt; (navegaci\u00f3n)"
  ],
  ...azul
});

// Fila 2

addClase({ name: "Cliente", x: 50, y: 420, w: 260, h: 360,
  atributos: [
    "+ Id: int",
    "+ Dni: string",
    "+ Nombre: string",
    "+ Telefono: string",
    "+ Email: string?",
    "+ DireccionDefault: string?",
    "+ NumeroDefault: int?",
    "+ DepartamentoDefault: string?",
    "+ IdCiudad: int?",
    "+ FechaCreacion: DateTime",
    "+ Activo: bool",
    "+ Ciudad: Ciudad? (navegaci\u00f3n)",
    "+ Ventas: ICollection&lt;Venta&gt; (navegaci\u00f3n)"
  ],
  ...azul
});

addClase({ name: "Venta", x: 350, y: 420, w: 270, h: 330,
  atributos: [
    "+ Id: int",
    "+ NumeroVenta: int",
    "+ Descuento: int",
    "+ Total: decimal",
    "+ MetodoPago: string",
    "+ TipoEntrega: string",
    "+ Notas: string?",
    "+ Estado: string",
    "+ FechaCreacion: DateTime",
    "+ IdDireccion: int?",
    "+ IdUsuario: int",
    "+ Direccion: Direccion? (navegaci\u00f3n)",
    "+ Usuario: Usuario? (navegaci\u00f3n)",
    "+ Detalles: ICollection&lt;DetalleVenta&gt; (navegaci\u00f3n)"
  ],
  ...azul
});

addClase({ name: "DetalleVenta", x: 660, y: 420, w: 250, h: 220,
  atributos: [
    "+ Id: int",
    "+ SubTotal: decimal",
    "+ Cantidad: int",
    "+ PrecioUnitario: decimal",
    "+ IdVenta: int",
    "+ IdProducto: int",
    "+ Venta: Venta? (navegaci\u00f3n)",
    "+ Producto: Producto? (navegaci\u00f3n)"
  ],
  ...azul
});

addClase({ name: "Direccion", x: 950, y: 420, w: 250, h: 220,
  atributos: [
    "+ Id: int",
    "+ Calle: string",
    "+ Numero: int",
    "+ Departamento: string?",
    "+ IdUsuario: int",
    "+ IdCiudad: int",
    "+ Usuario: Usuario? (navegaci\u00f3n)",
    "+ Ciudad: Ciudad? (navegaci\u00f3n)",
    "+ Ventas: ICollection&lt;Venta&gt; (navegaci\u00f3n)"
  ],
  ...azul
});

addClase({ name: "Ciudad", x: 1240, y: 420, w: 240, h: 190,
  atributos: [
    "+ Id: int",
    "+ NombreCiudad: string",
    "+ Cp: int",
    "+ IdProvincia: int",
    "+ Provincia: Provincia? (navegaci\u00f3n)",
    "+ Direcciones: ICollection&lt;Direccion&gt; (navegaci\u00f3n)",
    "+ Clientes: ICollection&lt;Cliente&gt; (navegaci\u00f3n)"
  ],
  ...azul
});

addClase({ name: "Provincia", x: 1520, y: 420, w: 220, h: 110,
  atributos: [
    "+ Id: int",
    "+ NombreProvincia: string",
    "+ Ciudades: ICollection&lt;Ciudad&gt; (navegaci\u00f3n)"
  ],
  ...azul
});

// ════════ INTERFACES DE SERVICIO (gris) ════════

addClase({ name: "ICategoriaService", x: 50, y: 840, w: 290, h: 220,
  metodos: [
    "+ ValidaCategoria(dto, idCategoria?): List&lt;string&gt;",
    "+ ObtenerTodosAsyncCategoria(): IEnumerable&lt;CategoriaDTO&gt;",
    "+ ObtenerPorIdAsyncCategoria(id): CategoriaDTO?",
    "+ CrearAsyncCategoria(categoria): CategoriaDTO",
    "+ ActualizarAsyncCategoria(id, categoria): CategoriaDTO?",
    "+ EliminarAsyncCategoria(id): bool",
    "+ PuedeEliminarAsync(id): bool"
  ],
  ...gris
});

addClase({ name: "IProductoService", x: 380, y: 840, w: 320, h: 260,
  metodos: [
    "+ ValidaProducto(dto, idProducto?): List&lt;string&gt;",
    "+ ObtenerTodosAsyncProducto(pag, size, cat, activo): (Items, Total)",
    "+ ObtenerPorIdAsyncProducto(id): ProductoDTO?",
    "+ CrearAsyncProducto(producto): ProductoDTO",
    "+ ActualizarAsyncProducto(id, producto): ProductoDTO?",
    "+ EliminarAsyncProducto(id): bool",
    "+ ToggleActivoAsyncProducto(id): ProductoDTO?",
    "+ VerificarStockProductoAsync(id, cant): StockVerificacionDTO",
    "+ ActualizarStockAsync(id, cant): bool"
  ],
  ...gris
});

addClase({ name: "IVentaService", x: 740, y: 840, w: 320, h: 260,
  metodos: [
    "+ RegistrarVentaAsync(idUsuario, ventaDto): VentaDTO",
    "+ ObtenerVentaPorIdAsync(id): VentaDTO?",
    "+ ObtenerHistorialVentasAsync(p, s, usuario, estado, d, h): (Items, Total)",
    "+ ActualizarEstadoVentaAsync(id, updateDto): VentaDTO?",
    "+ CancelarVentaAsync(id): VentaDTO?",
    "+ VerificarStockCarritoAsync(detalles): CarritoVerificacionDTO",
    "+ ObtenerVentasPorUsuarioAsync(id, p, s): VentaPagedResult",
    "+ ObtenerEstadisticasVentasAsync(d, h): object"
  ],
  ...gris
});

addClase({ name: "IAuthService", x: 1100, y: 840, w: 280, h: 160,
  metodos: [
    "+ RegisterAsync(request): AuthResult",
    "+ LoginAsync(request): AuthResult",
    "+ GetCurrentUserAsync(userId): UserDTO?"
  ],
  ...gris
});

addClase({ name: "IClienteService", x: 1420, y: 840, w: 290, h: 190,
  metodos: [
    "+ ObtenerTodosAsync(p, s, nombre, activo): (Items, Total)",
    "+ ObtenerPorIdAsync(id): ClienteDTO?",
    "+ BuscarPorDniAsync(dni): ClienteDTO?",
    "+ AgregarAsyncCliente(clienteDto): ClienteDTO",
    "+ ValidarDatosClienteAsync(dni, nombre, tel, email, idCiudad?, idExcluir?)"
  ],
  ...gris
});

// ════════ CONTROLADORES (amarillo) ════════

addClase({ name: "AuthController", estereotipo: "Controller", x: 50, y: 1170, w: 280, h: 200,
  atributos: [
    "~ _authService: IAuthService",
    "~ _validador: IValidadorAuth"
  ],
  metodos: [
    "+ POST register(request): AuthResult",
    "+ POST login(request): AuthResult",
    "+ POST logout(): IActionResult",
    "+ GET me(): UserDTO"
  ],
  ...amarillo
});

addClase({ name: "CategoriasController", estereotipo: "Controller", x: 370, y: 1170, w: 300, h: 200,
  atributos: [
    "~ _categoriaService: ICategoriaService",
    "~ _validador: IValidadorCategoria"
  ],
  metodos: [
    "+ GET obtenerTodas(): IEnumerable&lt;CategoriaDTO&gt;",
    "+ GET obtenerPorId(id): CategoriaDTO",
    "+ POST crear(categoria): CategoriaDTO",
    "+ PUT actualizar(id, categoria): CategoriaDTO",
    "+ DELETE eliminar(id)"
  ],
  ...amarillo
});

addClase({ name: "ProductosController", estereotipo: "Controller", x: 710, y: 1170, w: 310, h: 210,
  atributos: [
    "~ _productoService: IProductoService",
    "~ _validador: IValidadorProducto"
  ],
  metodos: [
    "+ GET obtenerTodos(page, size, cat, activo): PagedResult",
    "+ GET obtenerPorId(id): ProductoDTO",
    "+ POST crear(producto): ProductoDTO",
    "+ PUT actualizar(id, producto): ProductoDTO",
    "+ DELETE eliminar(id)",
    "+ PATCH toggleActivo(id)"
  ],
  ...amarillo
});

addClase({ name: "VentasController", estereotipo: "Controller", x: 1060, y: 1170, w: 330, h: 310,
  atributos: [
    "~ _ventaService: IVentaService",
    "~ _productoService: IProductoService",
    "~ _clienteService: IClienteService",
    "~ _validador: IValidadorVenta"
  ],
  metodos: [
    "+ POST registrar(ventaDto): VentaDTO",
    "+ GET obtenerPorId(id): VentaDTO",
    "+ GET historial(page, size, usuario, estado, fechas): PagedResult",
    "+ GET mis-ventas(page, size): PagedResult",
    "+ PUT estado(id, updateDto): VentaDTO",
    "+ PUT cancelar(id): VentaDTO",
    "+ GET verificar-stock(idProducto, cantidad)",
    "+ POST verificar-carrito(detalles)",
    "+ GET estadisticas(fechaDesde, fechaHasta)",
    "+ GET buscar-cliente(dni): ClienteDTO"
  ],
  ...amarillo
});

addClase({ name: "ClientesController", estereotipo: "Controller", x: 1430, y: 1170, w: 290, h: 180,
  atributos: [
    "~ _clienteService: IClienteService",
    "~ _validador: IValidadorCliente"
  ],
  metodos: [
    "+ GET buscar(nombre?)",
    "+ GET buscarPorDni(dni): ClienteDTO",
    "+ GET obtenerPorId(id): ClienteDTO",
    "+ POST agregar(clienteDto): ClienteDTO"
  ],
  ...amarillo
});

// ════════ VALIDADORES (violeta, interfaces) ════════

addClase({ name: "IValidadorAuth", x: 50, y: 1550, w: 270, h: 130,
  metodos: [
    "+ ValidarDatosRegistro(dto): List&lt;string&gt;",
    "+ ValidarDatosLogin(dto): List&lt;string&gt;"
  ],
  ...violeta
});

addClase({ name: "IValidadorCategoria", x: 360, y: 1550, w: 290, h: 130,
  metodos: [
    "+ ValidarDatosCategoria(dto): List&lt;string&gt;",
    "+ ValidaCategoria(dto, idCategoria?): List&lt;string&gt;"
  ],
  ...violeta
});

addClase({ name: "IValidadorProducto", x: 690, y: 1550, w: 300, h: 170,
  metodos: [
    "+ ValidarDatosProductoCreate(dto): List&lt;string&gt;",
    "+ ValidarDatosProductoUpdate(dto): List&lt;string&gt;",
    "+ ValidaProducto(dto, idProducto?): List&lt;string&gt;",
    "+ ValidarStock(idProducto, cantidad): List&lt;string&gt;"
  ],
  ...violeta
});

addClase({ name: "IValidadorVenta", x: 1030, y: 1550, w: 300, h: 150,
  metodos: [
    "+ ValidarDatosVenta(dto): List&lt;string&gt;",
    "+ ValidarDatosVentaCreate(dto, idUsuario): List&lt;string&gt;",
    "+ ValidarDatosVentaUpdate(dto): List&lt;string&gt;"
  ],
  ...violeta
});

addClase({ name: "IValidadorCliente", x: 1370, y: 1550, w: 290, h: 130,
  metodos: [
    "+ ValidarDatosCliente(dto): List&lt;string&gt;",
    "+ ValidarDatosCliente(dto, idCliente?): List&lt;string&gt;"
  ],
  ...violeta
});

// ═══════════════════════════════════════════════════════════════════════
//  RELACIONES
// ═══════════════════════════════════════════════════════════════════════

// Categoria 1 ── * Producto
addRel("cls_Categoria", "cls_Producto",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=ERone;endArrow=ERmany;exitX=1;exitY=0.5;entryX=0;entryY=0.3;",
  "1        *");

// Producto 1 ── * DetalleVenta
addRel("cls_Producto", "cls_DetalleVenta",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=ERone;endArrow=ERmany;exitX=1;exitY=0.5;entryX=0;entryY=0.3;",
  "1        *");

// Venta 1 ◆── * DetalleVenta (composición)
addRel("cls_Venta", "cls_DetalleVenta",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=diamondThin;startFill=1;endArrow=ERmany;exitX=0.5;exitY=1;entryX=1;entryY=0.5;",
  "1        *");

// Usuario 1 ── 1 Contacto
addRel("cls_Usuario", "cls_Contacto",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=ERone;endArrow=ERone;exitX=1;exitY=0.4;entryX=0;entryY=0.5;",
  "1        1");

// Usuario 1 ── * Venta
addRel("cls_Usuario", "cls_Venta",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=ERone;endArrow=ERmany;exitX=0.5;exitY=1;entryX=0;entryY=0.7;",
  "1        *");

// Usuario 1 ── * Direccion
addRel("cls_Usuario", "cls_Direccion",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=ERone;endArrow=ERmany;exitX=0.5;exitY=1;entryX=1;entryY=0;",
  "1        *");

// Provincia 1 ── * Ciudad
addRel("cls_Provincia", "cls_Ciudad",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=ERone;endArrow=ERmany;exitX=0;exitY=0.5;entryX=1;entryY=0.3;",
  "1        *");

// Ciudad 1 ── * Direccion
addRel("cls_Ciudad", "cls_Direccion",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=ERone;endArrow=ERmany;exitX=0;exitY=0.5;entryX=1;entryY=0.5;",
  "1        *");

// Ciudad 1 ── 0..1 Cliente
addRel("cls_Ciudad", "cls_Cliente",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=ERone;endArrow=ERzeroToOne;exitX=0;exitY=0.5;entryX=0;entryY=0.3;",
  "1        0..1");

// Venta * ── 1 Direccion
addRel("cls_Venta", "cls_Direccion",
  "edgeStyle=orthogonalEdgeStyle;html=1;startArrow=ERmany;endArrow=ERone;exitX=1;exitY=0.5;entryX=0;entryY=0.5;",
  "*        1");

// ════════ Controladores → Servicios (dependencia) ════════

addRel("cls_CategoriasController", "cls_ICategoriaService",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=0;exitY=0.2;entryX=1;entryY=0.8;",
  "depende");

addRel("cls_ProductosController", "cls_IProductoService",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=0;exitY=0.2;entryX=1;entryY=0.5;",
  "depende");

addRel("cls_VentasController", "cls_IVentaService",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=0;exitY=0.3;entryX=1;entryY=0.3;",
  "depende");

addRel("cls_AuthController", "cls_IAuthService",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=1;exitY=0.2;entryX=0;entryY=0.5;",
  "depende");

addRel("cls_ClientesController", "cls_IClienteService",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=1;exitY=0.3;entryX=0;entryY=0.5;",
  "depende");

// ════════ Controladores → Validadores (dependencia) ════════

addRel("cls_AuthController", "cls_IValidadorAuth",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=0.5;exitY=1;entryX=0.5;entryY=0;",
  "usa");

addRel("cls_CategoriasController", "cls_IValidadorCategoria",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=0.3;exitY=1;entryX=0.5;entryY=0;",
  "usa");

addRel("cls_ProductosController", "cls_IValidadorProducto",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=0.3;exitY=1;entryX=0.3;entryY=0;",
  "usa");

addRel("cls_VentasController", "cls_IValidadorVenta",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=0.5;exitY=1;entryX=0.5;entryY=0;",
  "usa");

addRel("cls_ClientesController", "cls_IValidadorCliente",
  "edgeStyle=orthogonalEdgeStyle;dashed=1;html=1;endArrow=open;endFill=0;exitX=0.7;exitY=1;entryX=0.5;entryY=0;",
  "usa");

// ════════ ENSAMBLAR ════════

output += cells.join("\n\n");
output += `

      </root>
    </mxGraphModel>
  </diagram>
</mxfile>`;

const outPath = "C:\\Users\\Usuario\\Desktop\\proyecto-SISIE\\docs\\diagramas\\diagrama-clases.drawio";
fs.writeFileSync(outPath, output, "utf-8");
console.log("Diagrama generado:", outPath);
console.log("Total celdas:", cells.length);
