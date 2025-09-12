<script lang="ts">
import Footer from '$lib/components/Footer.svelte';
import axios from 'axios';
import { onMount } from 'svelte';

const api = axios.create({
    baseURL: "https://localhost:7286",
    headers: {'Accept': 'application/json',
            'Content-Type': 'application/json'}
})

let page: number = 1;
let totpage: number = 0;
let message: string = '';
let isfound: boolean = false;
let prods: any[] = [];

const fetchProducts = (pg: any) => {
    message = "Please wait...";
    api.get(`/api/listproducts/${pg}`)
      .then((res: any) => {        
          prods = res.data.products;
          totpage = res.data.totpage;
          page = res.data.page;
          isfound = true;
      }, (error: any) => {
          message = error.response.data.message;
      });
      window.setTimeout(() => {
        message = '';
    }, 3000);              
  }

  onMount(() => {
    fetchProducts(page);
  })


  const firstPage = (event: any) => {
    event.preventDefault();
    page = 1;
    fetchProducts(page);
    return;
  }

  const nextPage = (event: any) => {        
    event.preventDefault();
    if (page === totpage) {
        return;
    } 
    let pg = page + 1;
    page = pg;
    fetchProducts(page);
 }

 const prevPage = (event: any) => {
    event.preventDefault();
    let pg = page - 1;
    page = pg;
    fetchProducts(page);
    return;
}

const lastPage = (event: any) => {
    event.preventDefault();
    page = totpage;
    fetchProducts(page);
    return;
}

function formatToDecimal(xval: any) {
  const formatter = new Intl.NumberFormat('en-US', {
    minimumFractionDigits: 2, // Ensures at least two decimal places
    maximumFractionDigits: 2, // Limits to two decimal places
  });
  return formatter.format(xval);
}

</script>

<div class="container-fluid mt-3">
    <h3>Products Catalog</h3>

    {#if isfound === false}
      <div class="text-left text-danger">{message}</div>
    {/if}

    <div class="card-group mt-4">
    {#each prods as product}
        
          <div class="card mx-2">
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
        <ul class="pagination mt-4">
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
