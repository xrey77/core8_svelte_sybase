<script lang="ts">
import Footer from '$lib/components/Footer.svelte';
import axios from 'axios';

const api = axios.create({
    baseURL: "https://localhost:7286",
    headers: {'Accept': 'application/json',
            'Content-Type': 'application/json'}
})

let page: number = 1;
let totpage: number = 0;
let message: string = '';
let search: any = '';
let isfound: boolean = false;
let prods: any[] = [];

  function formatToDecimal(xval: any){
    const formatter = new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });
    return formatter.format(xval);
  }
    
  async function searchProducts(pg: any, key: any) {
    message = "please wait...searching...";
    await api.get(`/api/searchproducts/${pg}/${key}`)
      .then((res: any) => {
          prods = res.data.products;
          totpage = res.data.totpage;
          page = res.data.page;
          isfound = true;
          if (totpage === 0) {
            message = "keyword not found....";
          }
      }, (error: any) => {
          if (error.response) {
            message = error.response.data.message;
          } else {
            message = error.message;
          }
          isfound = false;
          totpage = 0;
      });    
      window.setTimeout(() => {
        message = '';
      }, 3000);
  } 

  const submitSearchForm: SubmitEventHandler<SubmitEvent, HTMLFormElement> = (event: any) => {  
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const data = Object.fromEntries(formData.entries());  
    search = data.search;  
    searchProducts(page, search);
  }

  const firstPage = (event: any) => {
    event.preventDefault();
    page = 1;
    searchProducts(page, search);
    return;
  }

  const nextPage = async (event: any) => {        
    event.preventDefault();
    if (page === totpage) {
        return;
    } 
    let pg = page + 1;
    page = pg;
    await searchProducts(page, search);
  }

  const prevPage = (event: any) => {
    event.preventDefault();
    let pg = page - 1;
    page = pg;
    searchProducts(page, search);
    return;
  }

  const lastPage = (event: any) => {
    event.preventDefault();
    page = totpage;
    searchProducts(page, search);
    return;
  }

</script>

<div class="container-fluid mb-9">
    <h3 class="mt-3">Search Product</h3>
    {#if isfound === false} 
      <div class="text-left text-danger mb-2">{message}</div>
    {/if}
    
    <form onsubmit={submitSearchForm} class="row g-3" autocomplete="off">
        <div class="col-auto">
          <input type="text" class="form-control-sm" id="search" name="search" required placeholder="enter description key"/>
        </div>
        <div class="col-auto">
          <button type="submit" class="btn btn-primary btn-sm mb-3">search</button>
        </div>

    </form>

    <div class="card-group mb-4">

    {#each prods as product}
        <div class="card mx-1">
          <img src={product.productPicture} class="card-img-top" alt="..."/>
          <div class="card-body">
            <h5 class="card-title">Descriptions</h5>
            <p class="card-text">{product.descriptions}</p>
          </div>
          <div class="card-footer">
              <p class="card-text text-danger"><span class="text-dark">PRICE :</span>&nbsp;<strong>&#8369;{formatToDecimal(product.sellPrice)}</strong></p>
          </div>  
        </div>
    {/each}

    </div>    

    {#if totpage > 1}
     <nav aria-label="Page navigation example">
      <ul class="pagination">
        <li class="page-item"><button type="button" onclick={lastPage} class="page-link">Last</button></li>
        <li class="page-item"><button type="button" onclick={prevPage} class="page-link">Previous</button></li>
        <li id="next" class="page-item"><button type="button" onclick={nextPage} class="page-link">Next</button></li>
        <li class="page-item"><button type="button" onclick={firstPage} class="page-link">First</button></li>
        <li class="page-item page-link text-danger">Page&nbsp;{page} of&nbsp;{totpage}</li>
     </ul>
    </nav> 
  {/if}
</div>    
<div class="fixed-bottom mb-3">
  <Footer/>
</div>
