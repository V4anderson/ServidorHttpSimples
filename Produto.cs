class Produto
{
    public static List<Produto> Listagem {get; set;}
    public int Codigo{get; set;}
    public string Nome{get; set;}

    static Produto()
    {
        Produto.Listagem = new List<Produto>();
        Produto.Listagem.AddRange(new List<Produto>
        {
            new Produto{Codigo=1, Nome="CREME PARA PENTEAR NATUHAIR S.O.S CACHOS INTENSOS 1KG"},
            new Produto{Codigo=2, Nome="ÓLEO DE ALECRIM NATUHAIR 60ML"},
            new Produto{Codigo=3, Nome="GELATINA REDUTORA DE VOLUME SOS NATUHAIR ATIVADORA DE CACHOS 300G"},
            new Produto{Codigo=4, Nome="GELATINA BABOSA NATUHAIR 500G"},
            
        });
    }

}