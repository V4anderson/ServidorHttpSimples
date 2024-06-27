using System.Net;
using System.Net.Sockets;
using System.Text;

class ServidorHttp 
{
    private TcpListener Controlador {get ; set ;}
    private int Porta {get; set;}
    private int QtdeRequests {get; set;}
    private string HtmlExemplo {get; set;}

    private SortedList<string, string> TiposMime {get; set;}
    private SortedList<string, string> DiretoriosHosts {get; set;}

    public ServidorHttp(int porta = 8080) 
    {
        this.Porta = porta;
        this.CriarHtmlExemplo();
        this.PopularTiposMIME();
        this.PopularDiretoriosHosts();
        
        try 
        {
            this.Controlador = new TcpListener(IPAddress.Parse("127.0.0.1"),this.Porta);
            this.Controlador.Start();
            Console.WriteLine($"Servidor HTTP está rodando na porta {this.Porta}.");
            Console.WriteLine($"Para acessar, digite no navegador: http://localhost:{this.Porta}.");
            Task ServidorHttpTask = Task.Run(() => AguardarRequest());
            ServidorHttpTask.GetAwaiter().GetResult();
        }
        catch(Exception e) 
        {
            Console.WriteLine($"Erro ao iniciar o servidor na porta {this.Porta}:\n{e.Message}");
        }
    }
    private async Task AguardarRequest()
    {
        while (true) 
        {
            Socket conexao = await this.Controlador.AcceptSocketAsync();
            this.QtdeRequests++;
            Task task = Task.Run(() => ProcessarRequest(conexao, this.QtdeRequests));

        }
    }
    private void ProcessarRequest(Socket conexao, int numeroRequest) 
    {

        Console.WriteLine($"Processando request #{numeroRequest}...");
        if(conexao.Connected) 
        {
            byte[] bytesRequisicao = new byte[1024];
            conexao.Receive(bytesRequisicao, bytesRequisicao.Length, 0);
            string textoRequisicao = Encoding.UTF8.GetString(bytesRequisicao)
                .Replace((char)0,' ').Trim();
                if(textoRequisicao.Length > 0) 
                {

                    Console.WriteLine($"\n{textoRequisicao}\n");

                    string[] linhas = textoRequisicao.Split("\r\n");

                    int iPrimeiroEspaco = linhas[0].IndexOf(' ');
                    int iSegundoEspaco  = linhas[0].LastIndexOf(' ');
                    string metodoHttp   = linhas[0].Substring(0, iPrimeiroEspaco);
                    string recursoBuscado = linhas[0].Substring(
                        iPrimeiroEspaco + 1, iSegundoEspaco - iPrimeiroEspaco - 1);
                    if(recursoBuscado == "/") recursoBuscado = "/index.html";
                    string textoParametros = recursoBuscado.Contains("?") ?
                        recursoBuscado.Split("?")[1] : "";
                    SortedList<string, string> parametros = ProcessarParametros(textoParametros);
                    recursoBuscado = recursoBuscado.Split("?")[0];
                    string versaoHttp = linhas[0].Substring(iSegundoEspaco + 1);
                    iPrimeiroEspaco = linhas[1].IndexOf(' ');
                    string nomeHost = linhas[1].Substring(iPrimeiroEspaco + 1);

                    byte[] bytesCabecalho = null;
                    byte[] bytesConteudo = null;
                    FileInfo fiArquivo = new FileInfo(ObterCaminhoFisicoArquivo(nomeHost, recursoBuscado));
                    if(fiArquivo.Exists) 
                    {
                        if(TiposMime.ContainsKey(fiArquivo.Extension.ToLower())) 
                        {
                            if(fiArquivo.Extension.ToLower() == ".dhtml")
                            {
                            bytesConteudo = GerarHTMLDinamico(fiArquivo.FullName);
                            }
                            else
                            {
                            bytesConteudo = File.ReadAllBytes(fiArquivo.FullName);
                            }
                            
                            string tipoMime = TiposMime[fiArquivo.Extension.ToLower()];
                            bytesCabecalho = GerarCabecalho(versaoHttp,tipoMime,"200",bytesConteudo.Length);
                        }
                        else 
                        {
                            bytesConteudo = Encoding.UTF8.GetBytes(
                                "<h1>Erro 415 - Tipo de arquivo não suportado. </h1>"
                                );
                            bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset=utf-8",
                            "415",bytesConteudo.Length);
                        }
                    }
                    else 
                        {
                            bytesConteudo = Encoding.UTF8.GetBytes(
                                "<h1>Erro 404 - Arquivo Não Encontrado </h1>");
                            bytesCabecalho = GerarCabecalho(versaoHttp, "text/html; charset=utf-8",
                            "404", bytesConteudo.Length);
                        }
                    int bytesEnviados = conexao.Send(bytesCabecalho, bytesCabecalho.Length, 0);
                    bytesEnviados += conexao.Send(bytesConteudo, bytesConteudo.Length, 0);
                    conexao.Close();
                    Console.WriteLine($"\n{bytesEnviados} bytes enviados em resposta à requisição # {numeroRequest}.");
                }
        }
        Console.WriteLine($"\nRequest {numeroRequest} finalizado.");
    }
    public byte[] GerarCabecalho(string versaoHttp, string tipoMime,
    string codigoHttp, int qtdeBytes = 0)
    {
        StringBuilder texto = new StringBuilder();
        texto.Append($"{versaoHttp} {codigoHttp}{Environment.NewLine}");
        texto.Append($"Server: Servidor Http Simples 1.0{Environment.NewLine}");
        texto.Append($"Content-Type: {tipoMime}{Environment.NewLine}");
        texto.Append($"Content-Lenght: {qtdeBytes}{Environment.NewLine}{Environment.NewLine}");
        return Encoding.UTF8.GetBytes(texto.ToString());
    }
    private void CriarHtmlExemplo() {
        StringBuilder html = new StringBuilder();
        html.Append("<h1>Página Estática!</h1>");
        this.HtmlExemplo = html.ToString();
    }

    private void PopularTiposMIME() 
    {
        this.TiposMime = new SortedList<string, string>();
        this.TiposMime.Add(".html","text/html;charset=utf-8");
        this.TiposMime.Add(".htm","text/html; charset=utf-8");
        this.TiposMime.Add(".css","text/css");
        this.TiposMime.Add(".js","text/javascript");
        this.TiposMime.Add(".jpg","image/jpeg");
        this.TiposMime.Add(".dhtml","text/html;charset=utf-8");
    }

private void PopularDiretoriosHosts()
    {
        this.DiretoriosHosts = new SortedList<string, string>();
        this.DiretoriosHosts.Add("localhost", "C:\\Users\\Vandeson\\Desktop\\ServidorHttpSimples\\www\\localhost");
        this.DiretoriosHosts.Add("v4anderson.com", "C:\\Users\\Vandeson\\Desktop\\ServidorHttpSimples\\www\\v4anderson.com");
        this.DiretoriosHosts.Add("quitandaonline.com.br", "C:\\Users\\Vandeson\\Desktop\\ServidorHttpSimples\\www\\quitandaonline.com.br");
    }
public string ObterCaminhoFisicoArquivo(string host, string arquivo)
    {
        string diretorio = this.DiretoriosHosts[host.Split(":")[0]];
        string caminhoArquivo = diretorio + arquivo.Replace("/","\\");
        return caminhoArquivo;
    }

public  byte[] GerarHTMLDinamico(string caminhoArquivo)
{
    string coringa = "{{HtmlGerado}}";
    string HtmlModelo = File.ReadAllText(caminhoArquivo);
    StringBuilder HtmlGerado = new StringBuilder();
    HtmlGerado.Append("<ul>");
    foreach(var tipo in this.TiposMime.Keys)
    {
        HtmlGerado.Append($"<li>Arquivos com extensão {tipo} </li>");
    }
    HtmlGerado.Append("/<ul>");
    string TextoHtmlGerado = HtmlModelo.Replace(coringa, HtmlGerado.ToString());
    return Encoding.UTF8.GetBytes(TextoHtmlGerado, 0, TextoHtmlGerado.Length);
}
private SortedList<string, string> ProcessarParametros(string textoParametros)
    {
        SortedList<string, string> parametros = new SortedList<string, string>();
        return parametros;
    }
}
