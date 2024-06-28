using System.Text;

class PaginaProdutos : PaginaDinamica
{
    public override byte[] Get(SortedList<string, string> parametros)
    {
    String codigo = parametros.ContainsKey("id") ? 
        parametros["id"] : "";
        StringBuilder HtmlGerado = new StringBuilder();
        foreach(var p in Produto.Listagem)
        {
            bool negrito = (!string.IsNullOrEmpty(codigo) && codigo == p.Codigo.ToString());
            HtmlGerado.Append("<tr>");
            if(negrito) 
            {
                HtmlGerado.Append($"<td><b>{p.Codigo:D4}</b></td>");
                HtmlGerado.Append($"<td><b>{p.Nome}</b></td>");
            }
            else
            {
                HtmlGerado.Append($"<td>{p.Codigo:D4}</td>");
                HtmlGerado.Append($"<td>{p.Nome}</td>");
            }
            HtmlGerado.Append("</tr>");
        }
        string TextoHtmlGerado = this.HtmlModelo.Replace("{{HtmlGerado}}", HtmlGerado.ToString());
        return Encoding.UTF8.GetBytes(TextoHtmlGerado);
    }
}